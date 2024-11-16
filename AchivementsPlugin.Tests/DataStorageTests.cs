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
    private static readonly string _dataDir = Path.Combine(Path.GetTempPath(), "DataStorageTest");
    private readonly IContainer container;
    private readonly DataStorageSql _data;
    private readonly AchievementsPlugin _plugin;
    private readonly string _currentDir;

    public DataStorageTest()
    {

        _currentDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory("../../../../out-win-x64");

        // Fake Server
        var containerBuilder = new ContainerBuilder();

        var configLocations = ConfigurationLocations.FromOptions(null, null, null);
        var config = new ACServerConfiguration(null, configLocations, true, false);
        var startup = new AssettoServer.Startup(config);

        var services = new ServiceCollection();
        
        startup.ConfigureServices(services);
        startup.ConfigureContainer(containerBuilder);

        container = containerBuilder.Build(ContainerBuildOptions.IgnoreStartableComponents);

        if (container == null)
        {
            throw new Exception("Failed to initialize Autofac");
        }
        // End Fake Server

        using (var scope = container.BeginLifetimeScope())
        {
            _plugin = scope.Resolve<AchievementsPlugin>();
            _data = scope.Resolve<DataStorageSql>();
        }
    }

    [OneTimeSetUp]
    public void SetUp()
    {
        if (!Directory.Exists(_dataDir))
        {
            Directory.CreateDirectory(_dataDir);
        }
        Directory.SetCurrentDirectory(_currentDir);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        Directory.Delete(_dataDir, true);
        container.Dispose();
    }

    [Test]
    public void DataDirEqual()
    {
        Assert.That(_data.DataDir, Is.EqualTo(_dataDir));
    }

    [Test]
    public void ContainerBuild()
    {
        Assert.That(container.IsRegistered(typeof(EntryCarManager)), Is.EqualTo(true));
    }

    [Test]
    public void TableCreated()
    {
        _plugin.CreateTableIfNotExists();
        Assert.That(
            () => _data.ExecuteScalar<uint>("SELECT 1 FROM achievemts"),
            Throws.Nothing
        );

    }
}
