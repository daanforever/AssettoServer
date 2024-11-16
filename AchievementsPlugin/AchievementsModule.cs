using AssettoServer.Server.Configuration;
using AssettoServer.Server.Plugin;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace AchievementsPlugin;

public class AchievementsModule : AssettoServerModule<AchievementsConfiguration>
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<AchievementsPlugin>().AsSelf().As<IAssettoServerAutostart>().SingleInstance();
    }
}
