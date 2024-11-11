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
using Dapper;
using SimpleStatsPlugin;
using CommandLine;
using System.ComponentModel;
using Autofac;
using Microsoft.Data.Sqlite;

namespace AchievementsPlugin;

public class AchievementsPlugin : CriticalBackgroundService, IAssettoServerAutostart
{
    public readonly AchievementsConfiguration Configuration;
    private readonly EntryCarManager _entryCarManager;
    private readonly ACPluginLoader _loader;
    //private SimpleStatsData _data;
    private ISimpleStatsData _data;

    public AchievementsPlugin(
        AchievementsConfiguration configuration,
        EntryCarManager entryCarManager,
        //ACPluginLoader loader,
        //SimpleStatsData data,
        //SimpleStatsData data,
        //IEnumerable<IAssettoServerAutostart> autostartServices,
        //IEnumerable<ISimpleStatsData> data,
        ILifetimeScope lifetimeScope,
        IHostApplicationLifetime applicationLifetime
        ) : base(applicationLifetime)
    {
        Configuration = configuration;
        //_loader = loader;
        _entryCarManager = entryCarManager;
        _entryCarManager.ClientConnected += OnClientConnected;


        //if (lifetimeScope.IsRegistered<SimpleStatsData>())
        //{
        //    Log.Debug("SimpleStatsData Registered");
        //} else
        //{
        //    Log.Debug("SimpleStatsData NOT Registered");
        //}

        _data = lifetimeScope.Resolve<SimpleStatsData>();
        //_data = data;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Debug("Achievements: plugin autostart called");

        var sql = 
            """
            SELECT COUNT(*) FROM records;
            """;

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
