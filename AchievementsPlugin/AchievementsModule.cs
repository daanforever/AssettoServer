using AssettoServer.Server.Configuration;
using AssettoServer.Server.Plugin;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using SimpleStatsPlugin;

namespace AchievementsPlugin;

public class AchievementsModule : AssettoServerModule<AchievementsConfiguration>
{
    protected override void Load(ContainerBuilder builder)
    {
        //builder.Build().Resolve<SimpleStatsData>();
        builder.RegisterType<AchievementsPlugin>().AsSelf().As<IAssettoServerAutostart>().SingleInstance();
        //builder.RegisterType<AchievementsPlugin>(c => { var s = c.Resolve<SimpleStatsData>(); });
    }
}
