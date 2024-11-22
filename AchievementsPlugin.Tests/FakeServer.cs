using AssettoServer.Server.Configuration;
using Autofac.Builder;
using Autofac;
using DataStoragePlugin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AchievementsPlugin.Tests.DataStorageTest;
using System.Reflection;

namespace AchievementsPlugin.Tests
{
    public class LifetimeStub : IHostApplicationLifetime
    {
        private static readonly CancellationToken cancellationToken = new();

        CancellationToken IHostApplicationLifetime.ApplicationStarted => cancellationToken;
        CancellationToken IHostApplicationLifetime.ApplicationStopping => cancellationToken;
        CancellationToken IHostApplicationLifetime.ApplicationStopped => cancellationToken;

        void IHostApplicationLifetime.StopApplication() { }
    }

    public class FakeServer : IDisposable
    {
        private readonly string _workDir;
        private readonly string _currentDir;

        public readonly string DataDir;
        public DataStorageSql Data;
        public readonly IContainer Container;

        public readonly ILifetimeScope Scope;

        public FakeServer()
        {
            // Dereference it not possible unless you load this test through Assembly.Load
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            _currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).ToString();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            _workDir = Path.Combine(_currentDir, "../../../../out-win-x64");

            Directory.SetCurrentDirectory(_workDir);

            DataDir = Directory.CreateTempSubdirectory("_data.").ToString();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<LifetimeStub>().As<IHostApplicationLifetime>();

            {
                var configLocations = ConfigurationLocations.FromOptions(null, null, null);
                var config = new ACServerConfiguration(null, configLocations, true, false);
                var startup = new AssettoServer.Startup(config);

                var services = new ServiceCollection();

                startup.ConfigureServices(services);
                startup.ConfigureContainer(containerBuilder);
            }

            Container = containerBuilder.Build(ContainerBuildOptions.IgnoreStartableComponents);

            if (Container == null)
            {
                throw new Exception("Failed to initialize Autofac");
            }

            Scope = Container.BeginLifetimeScope();

            DataStorageConfiguration dsConfig = new() { DataDir = DataDir, Pooling = false };
            Data = Scope.Resolve<DataStorageSql>(new TypedParameter(typeof(DataStorageConfiguration), dsConfig));
        }

        /// <summary>
        /// Disposes of the <see cref='System.ComponentModel.Component'/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources associated with this component.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (this)
                {
                    Data.Dispose();
                    Scope.Dispose();
                    Container.Dispose();
                    Directory.Delete(DataDir, true);
                    Directory.SetCurrentDirectory(_currentDir);
                }
            }
        }
    }
}
