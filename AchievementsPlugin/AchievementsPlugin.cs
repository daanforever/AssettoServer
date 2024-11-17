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
        DataStorageSql dataStorage,
        IHostApplicationLifetime applicationLifetime
        ) : base(applicationLifetime)
    {
        Configuration = configuration;
        _entryCarManager = entryCarManager;
        _entryCarManager.ClientConnected += OnClientConnected;
        _data = dataStorage;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Debug("Achievements: plugin autostart called");

        var sql =
            """
            SELECT COUNT(*) FROM records;
            """;

        var count = _data.ExecuteScalar<Int32>(sql);

        Log.Debug("Achievements: found {Count} records", count);

        var type = typeof(IAchievement);
        var achievementClasses = type.Assembly.GetTypes()
            .Where(p => type.IsAssignableFrom(p) && !p.IsInterface);

        Log.Debug("Achievements: {Num} known achievements", achievementClasses.Count());

        return Task.CompletedTask;
    }

    public void CreateTableIfNotExists() {
        var sql =
            """

            CREATE TABLE IF NOT EXISTS achievements
            (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT,
                active INTEGER DEFAULT TRUE,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            );

            CREATE UNIQUE INDEX IF NOT EXISTS achievements_name
                ON achievements (name);

            CREATE TABLE IF NOT EXISTS players_achievements
            (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                player_id INTEGER NOT NULL,
                achievement_id INTEGER NOT NULL,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY(player_id) REFERENCES players(id),
                FOREIGN KEY(achievement_id) REFERENCES achievements(id)
            );

            CREATE UNIQUE INDEX IF NOT EXISTS players_achievements_player_id_achievement_id
                ON players_achievements (player_id, achievement_id);
            
            """;

        _data.Execute(sql);
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
