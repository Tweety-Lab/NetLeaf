using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NetLeaf.Bridge
{
    public static class Utility
    {
        [UnmanagedCallersOnly(EntryPoint = "RunMethod")]
        public static void RunMethod(IntPtr methodNamespace, IntPtr loadedAssemblies, int assembliesCount)
        {
            try
            {
                // Read method namespace string
                string methodNamespaceStr = null;
                if (methodNamespace != IntPtr.Zero)
                {
                    methodNamespaceStr = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                        ? Marshal.PtrToStringUni(methodNamespace)
                        : Marshal.PtrToStringUTF8(methodNamespace);
                }
                else
                {
                    Console.WriteLine("C#: Method namespace pointer is null");
                    return;
                }

                // Extract loaded assemblies
                string[] assemblies = new string[assembliesCount];
                if (loadedAssemblies != IntPtr.Zero && assembliesCount > 0)
                {
                    for (int i = 0; i < assembliesCount; i++)
                    {
                        IntPtr assemblyPathPtr = Marshal.ReadIntPtr(loadedAssemblies, i * IntPtr.Size);
                        if (assemblyPathPtr != IntPtr.Zero)
                        {
                            assemblies[i] = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                                ? Marshal.PtrToStringAnsi(assemblyPathPtr)
                                : Marshal.PtrToStringUTF8(assemblyPathPtr);
                        }
                        else
                        {
                            Console.WriteLine($"C#: Assembly pointer {i} is null");
                            assemblies[i] = null;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No assemblies provided or pointer is null");
                    return;
                }

                // Parse format: Namespace.Class.Method
                string[] parts = methodNamespaceStr.Split('.');
                if (parts.Length < 3)
                {
                    Console.WriteLine("Invalid format. Expected: Namespace.Class.Method");
                    return;
                }

                string @namespace = string.Join(".", parts.Take(parts.Length - 2));
                string className = parts[parts.Length - 2];
                string methodName = parts[parts.Length - 1];
                string fullTypeName = $"{@namespace}.{className}";

                foreach (string assemblyName in assemblies)
                {
                    if (string.IsNullOrEmpty(assemblyName)) continue;

                    string fullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), assemblyName);

                    Assembly assembly = Assembly.LoadFrom(fullPath);
                    Type type = assembly.GetType(fullTypeName);

                    if (type == null)
                    {
                        Console.WriteLine($"Type '{fullTypeName}' not found in assembly.");
                        continue;
                    }

                    MethodInfo method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if (method == null)
                    {
                        Console.WriteLine($"Method '{methodName}' not found in type '{fullTypeName}'.");
                        continue;
                    }

                    method.Invoke(null, null);
                    break; // We had a successful invoke, break out of the loop
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RunMethod: {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
