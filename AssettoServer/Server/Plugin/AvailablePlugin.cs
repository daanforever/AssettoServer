using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using McMaster.NETCore.Plugins;
using Serilog;

namespace AssettoServer.Server.Plugin;

public class AvailablePlugin
{
    private readonly PluginConfiguration _configuration;
    private readonly string _path;
    private readonly string Dll;

    public AvailablePlugin(PluginConfiguration configuration, PluginLoader loader, string path, string dll)
    {
        _configuration = configuration;
        _path = path;
        Dll = dll;
    }

    public Assembly Load()
    {
        AssemblyLoadContext.Default.Resolving += (context, name) =>
        {
            Assembly? result = null;
            var assemblyPath = $"{_path}/{name.Name}.dll";

            if (File.Exists(assemblyPath))
            {
                result = context.LoadFromAssemblyPath(assemblyPath);
            }

            return result;
        };

        return AssemblyLoadContext.Default.LoadFromAssemblyPath(Dll);
    }

    public void LoadExportedAssemblies()
    {
        foreach (var assemblyName in _configuration.ExportedAssemblies)
        {
            var fileName = System.IO.Path.GetFileName(assemblyName);
            var fullPath = System.IO.Path.Combine(_path, fileName);
            AssemblyLoadContext.Default.LoadFromAssemblyPath(fullPath);
        }
    }
}
