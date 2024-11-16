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
    public readonly string DataDir;
    private readonly SqliteConnection _sqlite;

    public DataStorageSql(DataStorageConfiguration configuration)
    {
        Log.Debug("DataStorageSql instance created from {Caller}", Caller());

        DataDir = configuration.DataDir;

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

    public int Execute(string query, object? param = null) {
        return _sqlite.Execute(query, param);
    }

    public T? ExecuteScalar<T>(string query, object? param = null)
    {
        return _sqlite.ExecuteScalar<T>(query, param);
    }
}
