using NUnit.Framework;
using AchievementsPlugin;
using Autofac;
using DataStoragePlugin;
using Autofac.Builder;
using AssettoServer.Server.Configuration;
using AssettoServer.Server;
using Microsoft.Extensions.Hosting;
using System.Collections;
using Microsoft.Extensions.DependencyInjection;

namespace AchievementsPlugin.Tests;

public class DataStorageTest
{
    private readonly FakeServer _fake;
    private readonly AchievementsPlugin _plugin;

    public DataStorageTest()
    {
        _fake = new FakeServer();
        _plugin = _fake.Scope.Resolve<AchievementsPlugin>(new TypedParameter(typeof(DataStorageSql), _fake.Data));
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _plugin.Dispose();
        _fake.Dispose();
    }

    [Test]
    public void DataDirEqual()
    {
        Assert.That(_fake.Data.DataDir, Is.EqualTo(_fake.DataDir));
    }

    [Test]
    public void ContainerBuild()
    {
        Assert.That(_fake.Scope.IsRegistered(typeof(EntryCarManager)), Is.EqualTo(true));
    }

    [Test]
    public void TableCreated()
    {
        _plugin.CreateTableIfNotExists();
        Assert.That(
            () => _fake.Data.ExecuteScalar<uint>("SELECT 1 FROM achievements"),
            Throws.Nothing
        );

    }
}
