using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetLeaf.Bridge
{
    /// <summary>
    /// Manages loaded assemblies.
    /// </summary>
    public static class Assemblies
    {
        public static List<Assembly> LoadedAssemblies = new List<Assembly>() {};

        public static void LoadAssembly(string path)
        {
            // Get path relative to executable
            string fullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", path);

            if (!File.Exists(fullPath))
                return;

            // Load as assembly
            Assembly assembly = Assembly.LoadFrom(fullPath);
        }
    }
}
