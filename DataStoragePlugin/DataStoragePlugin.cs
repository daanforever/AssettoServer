using AssettoServer.Server.Configuration;
using Microsoft.Data.Sqlite;
using System.Data.Common;
using Dapper;
using System.Runtime.CompilerServices;
using Serilog;
using System.Diagnostics;

namespace DataStoragePlugin;

public class DataStorageSql : IDisposable
{
    public readonly string DataDir;
    private readonly SqliteConnection _sqlite;
    private readonly object lockObj = new();

    public DataStorageSql(DataStorageConfiguration configuration)
    {
        Log.Debug("DataStorageSql instance created");

        DataDir = configuration.DataDir;

        if (!Directory.Exists(DataDir))
        {
            Directory.CreateDirectory(DataDir);
        }

        SQLitePCL.Batteries.Init();
        _sqlite = new SqliteConnection(GetConnectionString(configuration.Pooling));
    }

    private string GetConnectionString(bool pooling = true)
    {
        string baseConnectionString = "Data Source=" + Path.Combine(DataDir, "stats.sqlite");

        return new SqliteConnectionStringBuilder(baseConnectionString)
        {
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = pooling,
        }.ToString();
    }

    public int? Execute(string query, object? param = null) {
        return _sqlite.Execute(query, param);
    }

    public T? ExecuteScalar<T>(string query, object? param = null)
    {
        return _sqlite.ExecuteScalar<T>(query, param);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            lock (lockObj)
            {
                if (_sqlite != null)
                {
                    _sqlite.Close();
                    _sqlite.Dispose();
                }
            }
        }
    }
}
