using AssettoServer.Server.Configuration;
using Microsoft.Data.Sqlite;
using System.Data.Common;
using Dapper;
using System.Runtime.CompilerServices;
using Serilog;
using System.Diagnostics;

namespace DataStoragePlugin;

public class DataStorageSql
{
    private static DataStorageSql? _instance;

    private readonly string DataDir;
    private readonly SqliteConnection _sqlite;

    public static DataStorageSql SingleInstance(string dataDir)
    {
        _instance ??= new DataStorageSql(dataDir);

        return _instance;
    }

    public DataStorageSql(string dataDir)
    {
        Log.Debug("DataStorageSql instance created from {Caller}", Caller());

        DataDir = dataDir;

        if (!Directory.Exists(DataDir))
        {
            Directory.CreateDirectory(DataDir);
        }

        SQLitePCL.Batteries.Init();
        _sqlite = new SqliteConnection(GetConnectionString());
    }

    private string GetConnectionString()
    {
        return "Data Source=" + Path.Combine(DataDir, "stats.sqlite");
    }

    private string? Caller()
    {
        var methodInfo = new StackTrace().GetFrame(1)?.GetMethod();
        return methodInfo?.ReflectedType?.Name;
    }

    public int Execute(string query)
    {
        return _sqlite.Execute(query);
    }

    public int Execute(string query, object? param) {
        return _sqlite.Execute(query, param);
    }

    public T? ExecuteScalar<T>(string query, object? param)
    {
        return _sqlite.ExecuteScalar<T>(query, param);
    }
}
