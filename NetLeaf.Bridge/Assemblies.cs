using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NetLeaf.Bridge;

public static class Assemblies
{
    private class LoadedPlugin
    {
        public PluginLoadContext Context;
        public Assembly Assembly;
        public string Path;
    }

    private static List<LoadedPlugin> _loadedPlugins = new();

    public static IEnumerable<Assembly> LoadedAssemblies
    => _loadedPlugins.Select(p => p.Assembly);

    public static void LoadAssembly(string path)
    {
        string fullPath = Path.IsPathRooted(path)
            ? path
            : System.IO.Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", path);

        if (!File.Exists(fullPath))
        {
            Console.WriteLine($"[NetLeaf] Assembly not found: {fullPath}");
            return;
        }

        var context = new PluginLoadContext(fullPath);
        var assembly = context.LoadFromAssemblyPath(fullPath);

        _loadedPlugins.Add(new LoadedPlugin
        {
            Context = context,
            Assembly = assembly,
            Path = fullPath
        });

        Console.WriteLine($"[NetLeaf] Assembly loaded: {fullPath}");
    }

    public static void UnloadAssembly(string path)
    {
        var plugin = _loadedPlugins.Find(p => p.Path == path);
        if (plugin != null)
        {
            _loadedPlugins.Remove(plugin);
            plugin.Context.Unload();

            // Force GC to clean up, otherwise unloading won't complete
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Console.WriteLine($"[NetLeaf] Assembly unloaded: {path}");
        }
    }

    public static MethodInfo FindMethodInAssembly(Assembly assembly, string fullTypeName, int paramCount)
    {
        int lastDot = fullTypeName.LastIndexOf('.');
        if (lastDot == -1)
            return null;

        string typeName = fullTypeName.Substring(0, lastDot);
        string methodName = fullTypeName.Substring(lastDot + 1);

        Type type = assembly.GetType(typeName);
        if (type == null)
            return null;

        return type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                   .FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == paramCount);
    }

    public static Type FindTypeInLoadedAssemblies(string typeNamespace)
    {
        foreach (var plugin in _loadedPlugins)
        {
            Type type = plugin.Assembly.GetType(typeNamespace);
            if (type != null)
                return type;
        }
        return null;
    }
}
