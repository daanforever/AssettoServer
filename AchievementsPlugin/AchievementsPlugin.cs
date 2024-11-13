using AssettoServer.Commands;
using AssettoServer.Server.Configuration;
using AssettoServer.Server;
using AssettoServer.Server.Plugin;
using AssettoServer.Shared.Services;
using System.Diagnostics;
using AssettoServer.Network.Tcp;
using AssettoServer.Shared.Model;
using AssettoServer.Shared.Network.Packets.Outgoing;
using AssettoServer.Shared.Network.Packets.Shared;
using System.ComponentModel;
using System.Runtime.Loader;
using DataStoragePlugin;
using Microsoft.Extensions.Hosting;
using Serilog;
using McMaster.NETCore.Plugins;

namespace AchievementsPlugin;

public class AchievementsPlugin : CriticalBackgroundService, IAssettoServerAutostart
{
    public readonly AchievementsConfiguration Configuration;
    private readonly EntryCarManager _entryCarManager;
    private readonly DataStorageSql _data;

    public AchievementsPlugin(
        AchievementsConfiguration configuration,
        EntryCarManager entryCarManager,
        IHostApplicationLifetime applicationLifetime
        ) : base(applicationLifetime)
    {
        Configuration = configuration;
        _entryCarManager = entryCarManager;
        _entryCarManager.ClientConnected += OnClientConnected;
        _data = DataStorageSql.SingleInstance("data");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Debug("Achievements: plugin autostart called");

        //var sql = 
        //    """
        //    SELECT COUNT(*) FROM records;
        //    """;

        //var count = _data.DbConnection.ExecuteScalar<Int32>(sql);

        //Log.Debug("Achievements: found {Count} records", count);

        return Task.CompletedTask;
    }

    public void OnClientConnected(ACTcpClient client, EventArgs args)
    {
        client.LapCompleted += OnLapCompleted;
    }

    private void OnLapCompleted(ACTcpClient client, LapCompletedEventArgs args)
    {
        LapCompletedOutgoing result = args.Packet;
    }
}
