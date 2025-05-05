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
            string fullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", path);

            if (!File.Exists(fullPath))
                return;

            Assembly assembly = Assembly.LoadFrom(fullPath);
            LoadedAssemblies.Add(assembly);
        }

        public static MethodInfo FindMethodInAssembly(Assembly assembly, string fullTypeName, int paramCount)
        {
            Type type = assembly.GetType(fullTypeName);
            if (type == null)
                return null;

            return type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                       .FirstOrDefault(m => m.GetParameters().Length == paramCount);
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
