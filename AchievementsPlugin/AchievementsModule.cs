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

    //public override void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    //{
    //}

    protected override void Load(ContainerBuilder builder)
    {
        //Log.Debug("-----");

        //foreach (var a in AssemblyLoadContext.Default.Assemblies.Where(
        //    x => x.GetName().Name.EndsWith("Plugin")
        //))
        //{
        //    Log.Debug("{A}", a);
        //}

        //Log.Debug("-----");

        builder.RegisterType<AchievementsPlugin>().AsSelf().As<IAssettoServerAutostart>().SingleInstance();
    }
}
