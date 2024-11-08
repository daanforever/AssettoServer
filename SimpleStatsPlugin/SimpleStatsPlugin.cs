using AssettoServer.Commands;
using AssettoServer.Server.Configuration;
using AssettoServer.Server;
using AssettoServer.Server.Plugin;
using AssettoServer.Shared.Services;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Diagnostics;
using AssettoServer.Network.Tcp;
using AssettoServer.Shared.Model;
using DotNext;
using AssettoServer.Shared.Network.Packets.Outgoing;
using AssettoServer.Shared.Network.Packets.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleStatsPlugin;

public class SimpleStatsPlugin : CriticalBackgroundService, IAssettoServerAutostart
{
    public readonly SimpleStatsConfiguration Configuration;
    private readonly ACServerConfiguration _serverConfig;
    private readonly EntryCarManager _entryCarManager;
    private readonly SimpleStatsData _data;

    public SimpleStatsPlugin(
        SimpleStatsConfiguration configuration,
        ACServerConfiguration serverConfig,
        EntryCarManager entryCarManager,
        IHostApplicationLifetime applicationLifetime
        ) : base(applicationLifetime)
    {
        Configuration = configuration;
        _serverConfig = serverConfig;
        _entryCarManager = entryCarManager;
        _entryCarManager.ClientConnected += OnClientConnected;
        _data = new SimpleStatsData(this, serverConfig);

        Log.Debug("SimpleStats plugin constructor called! Hello: {Hello}", configuration.Hello);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Debug("SimpleStats plugin autostart called");

        return Task.CompletedTask;
    }

    public void OnClientConnected(ACTcpClient client, EventArgs args)
    {
        client.ChatMessageReceived += OnChatMessageReceived;
        client.LapCompleted += OnLapCompleted;
    }

    public void OnChatMessageReceived(ACTcpClient client, ChatMessageEventArgs args)
    {
        Log.Debug("SimpleStats: chat message received");
        
    }

    private void OnLapCompleted(ACTcpClient client, LapCompletedEventArgs args)
    {
        LapCompletedOutgoing result = args.Packet;

        var message = $"{Utils.LapTimeFormat(result.LapTime)} {client.Name} {client.EntryCar.Model}";

        uint oldPB = _data.GetPB(client);

        if (args.Packet.Cuts == 0 && (oldPB == 0 || result.LapTime < oldPB))
        {
            var m = $"PB: {message}";

            Log.Information(m);

            _entryCarManager.BroadcastPacket(new ChatMessage { SessionId = 255, Message = m });

            SaveNewPB(client, result);
        } else if (args.Packet.Cuts == 0)
        {
            float diff = ((float)result.LapTime - (float)oldPB) / 1000;
            var loss = $"Loss {diff:0.000} sec from PB {Utils.LapTimeFormat(oldPB)}";
            client.SendPacket(new ChatMessage { SessionId = 255, Message = loss });
        }
        else 
        {
            Log.Information("Lap completed: {Cuts} cuts {Message}", args.Packet.Cuts, message);
        }
    }

    private void SaveNewPB(ACTcpClient client, LapCompletedOutgoing result)
    {
        _data.PutPB(client, result);
    }
}
