using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NetLeaf.Bridge
{
    public static class Assemblies
    {
        public static List<Assembly> LoadedAssemblies = new List<Assembly>();

        public static void LoadAssembly(string path)
        {
            string fullPath;

            // If the path is already absolute, use it directly
            if (Path.IsPathRooted(path))
            {
                fullPath = path;
            }
            else
            {
                // Otherwise, resolve it relative to the executing assembly
                string baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
                fullPath = Path.Combine(baseDir, path);
            }

            if (!File.Exists(fullPath))
            {
                Console.WriteLine($"[NetLeaf] Assembly not found: {fullPath}");
                return;
            }

            Assembly assembly = Assembly.LoadFrom(fullPath);
            LoadedAssemblies.Add(assembly);

            Console.WriteLine($"[NetLeaf] Assembly loaded: {fullPath}");
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
            foreach (var assembly in LoadedAssemblies)
            {
                Type type = assembly.GetType(typeNamespace);
                if (type != null)
                    return type;
            }
            return null;
        }
    }
}
