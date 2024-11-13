using AssettoServer.Server.Plugin;
using Autofac;
using System.Reflection;
using System.Runtime.Loader;

namespace DataStoragePlugin;

public class DataStorageModule : AssettoServerModule
{
    protected override void Load(ContainerBuilder builder)
    {
        //builder.RegisterType<DataStorage>().AsSelf().SingleInstance();
    }
}
