using AssettoServer.Server.Plugin;
using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleStatsPlugin;

public class SimpleStatsModule : AssettoServerModule<SimpleStatsConfiguration>
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<SimpleStatsData>().AsSelf().SingleInstance();
        builder.RegisterType<SimpleStatsPlugin>().AsSelf().As<IAssettoServerAutostart>().SingleInstance();
    }
}
