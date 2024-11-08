using AssettoServer.Network.Tcp;
using AssettoServer.Server.Configuration;
using AssettoServer.Shared.Network.Packets.Outgoing;
using Dapper;
using Microsoft.Data.Sqlite;
using Serilog;

namespace SimpleStatsPlugin;

public class TimeStampedModel
{
    public DateTime CreatedAt { set; get; }
}

public class Track : TimeStampedModel
{
    public required string Name { set; get; }
    public required string Config { set; get; }
}

public class Car : TimeStampedModel
{
    public required string Name { set; get; }
}

public class Player : TimeStampedModel
{
    public required string HashedGUID { set; get; }
    public required string Name { set; get; }
}

public class Record : TimeStampedModel
{
    public required Track Track { set; get; }
    public required Car Car { set; get; }
    public required Player Player { set; get; }
    public required UInt32 LapTime { set; get; }
}

public class SimpleStatsData
{
    private readonly SimpleStatsPlugin _plugin;
    private readonly ACServerConfiguration _serverConfig;
    private readonly SqliteConnection _dbConnection;

    public SimpleStatsData(SimpleStatsPlugin plugin, ACServerConfiguration serverConfig) { 
        _plugin = plugin;
        _serverConfig = serverConfig;

        _dbConnection = new SqliteConnection(GetConnectionString());
        Prepare();
    }

    private string GetConnectionString()
    {
        return "Data Source=" + Path.Combine(_plugin.Configuration.DataDir, "stats.sqlite");
    }

    private void Prepare()
    {
        if (!Directory.Exists(_plugin.Configuration.DataDir))
        {
            Directory.CreateDirectory(_plugin.Configuration.DataDir);
        }

        SQLitePCL.Batteries.Init();
        CreateTableIfNotExists();
    }

    private void CreateTableIfNotExists()
    {
        var sql =
            """
            CREATE TABLE IF NOT EXISTS tracks
            (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                track TEXT NOT NULL,
                config TEXT NOT NULL,
                name TEXT NOT NULL,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS cars
            (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                model TEXT NOT NULL,
                name TEXT,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS players
            (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                hashedGUID TEXT NOT NULL,
                name TEXT NOT NULL,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS records
            (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                track_id INTEGER NOT NULL,
                car_id INTEGER NOT NULL,
                player_id INTEGER NOT NULL,
                laptime INTEGER NOT NULL CHECK (laptime > 0),
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY(track_id) REFERENCES tracks(id),
                FOREIGN KEY(car_id) REFERENCES cars(id),
                FOREIGN KEY(player_id) REFERENCES players(id)
            );

            CREATE INDEX IF NOT EXISTS records_track_car_player
            ON records (track_id, car_id, player_id);
            
            """;

        _dbConnection.Execute(sql);
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
            """;

        var param = new {
            _serverConfig.Server.Track,
            _serverConfig.Server.TrackConfig,
            client.EntryCar.Model,
            client.HashedGuid,
            client.Name
        };

        return _dbConnection.ExecuteScalar<uint>(sql, param);
    }

    public void PutPB(ACTcpClient client, LapCompletedOutgoing result)
    {
        if (result.LapTime != 0) {
            Insert(GetTrackID(), GetCarID(client), GetPlayerID(client), result.LapTime);
        }
    }

    private int GetTrackID()
    {
        var sql =
            """
            SELECT id FROM tracks WHERE track = @Track AND config = @TrackConfig
            """;
        var param = new { _serverConfig.Server.Track, _serverConfig.Server.TrackConfig };

        Int32 id = 0;

        try
        {
            id = _dbConnection.QuerySingle<int>(sql, param);
        }
        catch (InvalidOperationException)
        {
            CreateTrack();
            id = _dbConnection.QuerySingle<int>(sql, param);
        }

        return id;
    }

    private void CreateTrack()
    {
        var sql =
            """
            INSERT INTO tracks (track, config, name) VALUES (@Track, @TrackConfig, @FullTrackName)
            """;
        var param = new {
            _serverConfig.Server.Track, 
            _serverConfig.Server.TrackConfig,
            _serverConfig.FullTrackName
        };

        var rowsAffected = _dbConnection.Execute(sql, param);
        Log.Debug("Insert into tracks {Num} rows: {Params}", rowsAffected, param);
    }

    private int GetCarID(ACTcpClient client)
    {
        var sql =
            """
            SELECT id FROM cars WHERE model = @Model
            """;
        var param = new { client.EntryCar.Model };

        Int32 id = 0;

        try
        {
            id = _dbConnection.QuerySingle<int>(sql, param);
        }
        catch (InvalidOperationException)
        {
            CreateCar(client);
            id = _dbConnection.QuerySingle<int>(sql, param);
        }

        return id;
    }

    private void CreateCar(ACTcpClient client)
    {
        var sql =
            """
            INSERT INTO cars (model) VALUES (@Model)
            """;
        var param = new
        {
            client.EntryCar.Model
        };

        var rowsAffected = _dbConnection.Execute(sql, param);
        Log.Debug("Insert into cars {Num} rows: {Params}", rowsAffected, param);
    }

    private int GetPlayerID(ACTcpClient client)
    {
        var sql =
            """
            SELECT id FROM players WHERE hashedGUID = @HashedGuid AND name = @Name
            """;
        var param = new { client.HashedGuid, client.Name };

        Int32 id = 0;

        try
        {
            id = _dbConnection.QuerySingle<int>(sql, param);
        }
        catch (InvalidOperationException)
        {
            CreatePlayer(client);
            id = _dbConnection.QuerySingle<int>(sql, param);
        }

        return id;
    }

    private void CreatePlayer(ACTcpClient client)
    {
        var sql =
            """
            INSERT INTO players (hashedGUID, name) VALUES (@HashedGuid, @Name)
            """;
        var param = new
        {
            client.HashedGuid, client.Name
        };

        var rowsAffected = _dbConnection.Execute(sql, param);
        Log.Debug("Insert into players {Num} rows: {Params}", rowsAffected, param);
    }

    private void Insert(Int32 TrackID, Int32 CarID, Int32 PlayerID, UInt32 LapTime)
    {
        var sql =
        """
            INSERT INTO records (track_id, car_id, player_id, laptime)
            VALUES (@TrackID, @CarID, @PlayerID, @LapTime)
        """;

        var param = new { TrackID, CarID, PlayerID, LapTime };

        var rowsAffected = _dbConnection.Execute(sql, param);
        Log.Debug("Insert into records {Num} rows: {Params}", rowsAffected, param);
    }
}
