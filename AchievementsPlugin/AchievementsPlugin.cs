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
using SimpleStatsPlugin;
using Microsoft.Extensions.Hosting;
using Serilog;
using McMaster.NETCore.Plugins;
using AchievementsPlugin.Achievements;

namespace AchievementsPlugin;

public class AchievementsPlugin : CriticalBackgroundService, IAssettoServerAutostart
{
    private readonly AchievementsConfiguration _configuration;
    private readonly EntryCarManager _entryCarManager;

    public readonly DataStorageSql DataStorage;
    public readonly List<object> Achievements = [];
    public readonly SimpleStats Stats;

    public AchievementsPlugin(
        AchievementsConfiguration configuration,
        EntryCarManager entryCarManager,
        DataStorageSql dataStorage,
        SimpleStats stats,
        IHostApplicationLifetime applicationLifetime
        ) : base(applicationLifetime)
    {
        _configuration = configuration;
        _entryCarManager = entryCarManager;
        DataStorage = dataStorage;
        Stats = stats;

        CreateTableIfNotExists();

        var type = typeof(IAchievement);
        var achievementClasses = type.Assembly.GetTypes()
            .Where(p => type.IsAssignableFrom(p) && !p.IsInterface);

        foreach (var achievementClass in achievementClasses)
        {
            var instance = Activator.CreateInstance(achievementClass, this);

            if (instance != null) {
                Achievements.Add(instance);
            }
        }

    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (Achievements != null)
        {
            Log.Debug("Achievements: registered {Num} achievement(s)", Achievements.Count);
        } else
        {
            Log.Warning("Achiements: achievements not found!");
        }

        return Task.CompletedTask;
    }

    public void CreateTableIfNotExists() {
        var sql =
            """

            CREATE TABLE IF NOT EXISTS achievements
            (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                class TEXT,
                active INTEGER DEFAULT TRUE,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            );

            CREATE UNIQUE INDEX IF NOT EXISTS achievements_name
                ON achievements (class);

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

        DataStorage.Execute(sql);
    }

    internal async Task Earn(IAchievement achievement, ACTcpClient player)
    {
        await Task.Run(() =>
        {
            if (!IsObtained(achievement, player))
            {
                Obtain(achievement, player);
                Announce(achievement, player);
            }
        });
    }

    private bool IsObtained(IAchievement achievement, ACTcpClient player)
    {
        var sql = """
            SELECT
                1
            FROM
                players_achievements pa
            INNER JOIN
                achievements a ON a.id = pa.achievement_id
            INNER JOIN
                players p ON p.id = pa.player_id
            WHERE
                a.class = @Achievement
                AND
                p.hashedGUID = @HashedGUID
                AND
                p.name = @PlayerName
            """;

        var param = new {
            Achievement = achievement.GetType().Name,
            @HashedGUID = player.HashedGuid,
            @PlayerName = player.Name
        }; 

        return DataStorage.ExecuteScalar<bool>(sql, param);
    }

    private void Obtain(IAchievement achievement, ACTcpClient player)
    {
        var sql = """
            INSERT INTO players_achievements (achievement_id, player_id)
            SELECT a.id, p.id
            FROM achievements a
            LEFT JOIN players p
            WHERE
              a.class = @Achievement
              AND
              p.hashedGUID = @HashedGUID
              AND
              p.name = @PlayerName
            """;

        var param = new
        {
            Achievement = achievement.GetType().Name,
            HashedGUID = player.HashedGuid,
            PlayerName = player.Name
        };

        DataStorage.Execute(sql, param);
    }

    private void Announce(IAchievement achievement, ACTcpClient player)
    {
        string message = $"{player.Name} earned achievement: <{achievement.Name}>";
        _entryCarManager.BroadcastPacket(new ChatMessage { SessionId = 255, Message = message });
    }
}
