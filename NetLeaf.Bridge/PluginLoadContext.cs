using System.Reflection;
using System.Runtime.Loader;

namespace NetLeaf.Bridge;

public class PluginLoadContext : AssemblyLoadContext
{
    private AssemblyDependencyResolver _resolver;

    public PluginLoadContext(string pluginPath) : base(isCollectible: true)
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName) ?? throw new Exception("Failed to create instance.");
        if (assemblyPath != null)
            return LoadFromAssemblyPath(assemblyPath);

        return null;
    }
}