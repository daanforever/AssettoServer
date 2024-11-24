using AssettoServer.Server.Plugin;
using Autofac;
using DataStoragePlugin;
using Microsoft.Extensions.DependencyInjection;
using NodaTime.TimeZones;
using System.Reflection;
using System.Runtime.Loader;

namespace SimpleStatsPlugin;

public class SimpleStatsModule : AssettoServerModule<SimpleStatsConfiguration>
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<SimpleStatsData>().AsSelf().SingleInstance();
        builder.RegisterType<SimpleStats>().AsSelf().As<IAssettoServerAutostart>().SingleInstance();
    }
}
