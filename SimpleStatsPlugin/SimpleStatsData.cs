using AssettoServer.Network.Tcp;
using AssettoServer.Server.Configuration;
using AssettoServer.Server.Plugin;
using AssettoServer.Shared.Network.Packets.Outgoing;
using AssettoServer.Shared.Services;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace SimpleStatsPlugin;

public class SimpleStatsData
{
    public SqliteConnection DbConnection { get; }

    private readonly SimpleStatsConfiguration _pluginConfig;
    private readonly ACServerConfiguration _serverConfig;

    public SimpleStatsData(
        SimpleStatsConfiguration pluginConfig, 
        ACServerConfiguration serverConfig,
        IHostApplicationLifetime applicationLifetime
    ) : base(applicationLifetime) {
        _pluginConfig = pluginConfig;
        _serverConfig = serverConfig;
        DbConnection = new SqliteConnection(GetConnectionString());
    }

    public string GetConnectionString()
    {
        return "Data Source=" + Path.Combine(_pluginConfig.DataDir, "stats.sqlite");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Debug("SimpleStatsPlugin autostart called");

        if (!Directory.Exists(_pluginConfig.DataDir))
        {
            Directory.CreateDirectory(_pluginConfig.DataDir);
        }

        SQLitePCL.Batteries.Init();
        CreateTableIfNotExists();

        return Task.CompletedTask;
    }

    public void CreateTableIfNotExists()
    {
        var sql =
            """
            CREATE TABLE IF NOT EXISTS tracks
            (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                track TEXT NOT NULL,
                config TEXT NOT NULL,
                name TEXT,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            );

            CREATE UNIQUE INDEX IF NOT EXISTS tracks_track_config
            ON tracks (track, config);

            CREATE TABLE IF NOT EXISTS cars
            (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                model TEXT NOT NULL,
                name TEXT,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            );

            CREATE UNIQUE INDEX IF NOT EXISTS cars_model
            ON cars (model);

            CREATE TABLE IF NOT EXISTS players
            (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                hashedGUID TEXT NOT NULL,
                name TEXT NOT NULL,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            );

            CREATE UNIQUE INDEX IF NOT EXISTS players_guid_name
            ON players (hashedGUID, name);

            CREATE TABLE IF NOT EXISTS records
            (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                track_id INTEGER NOT NULL,
                car_id INTEGER NOT NULL,
                player_id INTEGER NOT NULL,
                laptime INTEGER NOT NULL CHECK (laptime > 0),
                cuts INTEGER NOT NULL,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY(track_id) REFERENCES tracks(id),
                FOREIGN KEY(car_id) REFERENCES cars(id),
                FOREIGN KEY(player_id) REFERENCES players(id)
            );

            CREATE INDEX IF NOT EXISTS records_track_car_player
            ON records (track_id, car_id, player_id);
            
            """;

        DbConnection.Execute(sql);
    }

    public uint GetPB(ACTcpClient client)
    {
        var sql =
            """
            SELECT min(lapTime)
            FROM records
            INNER JOIN tracks ON tracks.id = records.track_id
            INNER JOIN cars ON cars.id = records.car_id
            INNER JOIN players ON players.id = records.player_id
            WHERE
                tracks.track = @Track AND tracks.config = @TrackConfig
                AND
                cars.model = @Model
                AND
                players.hashedGUID = @HashedGuid AND players.name = @Name
                AND records.cuts = 0
            """;

        var param = new {
            _serverConfig.Server.Track,
            _serverConfig.Server.TrackConfig,
            client.EntryCar.Model,
            client.HashedGuid,
            client.Name
        };

        return DbConnection.ExecuteScalar<uint>(sql, param);
    }

    public void SaveResult(ACTcpClient client, LapCompletedOutgoing result)
    {
        if (result.LapTime != 0) {
            Insert(
                new {
                    _serverConfig.Server.Track,
                    _serverConfig.Server.TrackConfig,
                    client.EntryCar.Model,
                    client.HashedGuid,
                    client.Name,
                    result.LapTime,
                    result.Cuts
                }
            );
        }
    }

    private void Insert(object param)
    {
        var sql =
        """
        INSERT OR IGNORE INTO tracks (track, config) VALUES (@Track, @TrackConfig);
        INSERT OR IGNORE INTO cars (model) VALUES (@Model);
        INSERT OR IGNORE INTO players (hashedGUID, name) VALUES (@HashedGuid, @Name);
        INSERT INTO records (track_id, car_id, player_id, laptime, cuts)
            SELECT tracks.id, cars.id, players.id, @LapTime, @Cuts
            FROM tracks
            LEFT JOIN cars
            LEFT JOIN players
            WHERE
              tracks.track = @Track AND tracks.config = @TrackConfig
              AND
              cars.model = @Model
              AND
              players.hashedGUID = @HashedGuid AND players.Name = @Name
        """;

        var rowsAffected = DbConnection.Execute(sql, param);
        Log.Debug("Insert into records {Num} rows: {Params}", rowsAffected, param);
    }
}
