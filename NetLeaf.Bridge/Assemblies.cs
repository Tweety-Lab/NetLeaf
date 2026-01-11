using System.Reflection;

namespace NetLeaf.Bridge;

public static class Assemblies
{
    /// <summary>
    /// Represents a loaded plugin/assembly.
    /// </summary>
    private class LoadedPlugin
    {
        public PluginLoadContext Context;
        public Assembly Assembly;
        public string Path;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadedPlugin"/> class.
        /// </summary>
        public LoadedPlugin(PluginLoadContext context, Assembly assembly, string path) => (Context, Assembly, Path) = (context, assembly, path);
    }

    private static readonly List<LoadedPlugin> _loadedPlugins = new();

    /// <summary>
    /// All currently loaded assemblies.
    /// </summary>
    public static IEnumerable<Assembly> LoadedAssemblies => _loadedPlugins.Select(p => p.Assembly);

    /// <summary>
    /// Load an assembly.
    /// </summary>
    /// <param name="path">Path to the assembly.</param>
    public static void LoadAssembly(string path)
    {
        string fullPath = Path.IsPathRooted(path) ? path : System.IO.Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", path);

        if (!File.Exists(fullPath))
        {
            Console.WriteLine($"[NetLeaf] Assembly not found: {fullPath}");
            return;
        }

        var context = new PluginLoadContext(fullPath);
        var assembly = context.LoadFromAssemblyPath(fullPath);

        _loadedPlugins.Add(new LoadedPlugin(context, assembly, fullPath));

        Console.WriteLine($"[NetLeaf] Assembly loaded: {fullPath}");
    }

    /// <summary>
    /// Unload an assembly.
    /// </summary>
    /// <param name="path">Path to the assembly.</param>
    public static void UnloadAssembly(string path)
    {
        var plugin = _loadedPlugins.Find(p => p.Path == path);
        if (plugin != null)
        {
            _loadedPlugins.Remove(plugin);
            plugin.Context.Unload();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Console.WriteLine($"[NetLeaf] Assembly unloaded: {path}");
        }
    }

    public static MethodInfo? FindMethod(string fullTypeName, int paramCount)
    {
        foreach (var method in EnumerateMethods(fullTypeName, paramCount))
        {
            if (method != null)
                return method;
        }

        return null;
    }

    private static IEnumerable<MethodInfo?> EnumerateMethods(string fullTypeName, int paramCount)
    {
        // Check loaded plugin assemblies first
        foreach (var asm in LoadedAssemblies)
            yield return TryGetMethod(asm, fullTypeName, paramCount);

        // Then fallback to all AppDomain assemblies
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            yield return TryGetMethod(asm, fullTypeName, paramCount);
    }

    private static MethodInfo? TryGetMethod(Assembly assembly, string fullTypeName, int paramCount)
    {
        int lastDot = fullTypeName.LastIndexOf('.');
        if (lastDot == -1)
            return null;

        string typeName = fullTypeName.Substring(0, lastDot);
        string methodName = fullTypeName.Substring(lastDot + 1);

        Type? type = assembly.GetType(typeName);
        if (type == null)
            return null;

        return type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                   .FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == paramCount);
    }

    public static Type? FindTypeInLoadedAssemblies(string typeNamespace)
    {
        foreach (var plugin in _loadedPlugins)
        {
            var type = plugin.Assembly.GetType(typeNamespace);
            if (type != null)
                return type;
        }

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = asm.GetType(typeNamespace);
            if (type != null)
                return type;
        }

        return null;
    }
}
