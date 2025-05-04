using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NetLeaf.Bridge
{
    public static class Methods
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

                // Parse: Namespace.Class.Method(arg1, arg2, ...)
                int parenStart = methodNamespaceStr.IndexOf('(');
                int parenEnd = methodNamespaceStr.LastIndexOf(')');

                if (parenStart == -1 || parenEnd == -1 || parenEnd <= parenStart)
                {
                    Console.WriteLine("Invalid format. Expected: Namespace.Class.Method(arg1, arg2, ...)");
                    return;
                }

                string fullMethodPath = methodNamespaceStr.Substring(0, parenStart);
                string argsStr = methodNamespaceStr.Substring(parenStart + 1, parenEnd - parenStart - 1);

                string[] pathParts = fullMethodPath.Split('.');
                if (pathParts.Length < 3)
                {
                    Console.WriteLine("Invalid format. Expected: Namespace.Class.Method");
                    return;
                }

                string @namespace = string.Join(".", pathParts.Take(pathParts.Length - 2));
                string className = pathParts[^2];
                string methodName = pathParts[^1];
                string fullTypeName = $"{@namespace}.{className}";

                // Parse arguments as strings
                string[] argStrings = argsStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

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

                    // Try to find a static method with matching argument count
                    MethodInfo method = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                        .FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == argStrings.Length);

                    if (method == null)
                    {
                        Console.WriteLine($"Method '{methodName}' with {argStrings.Length} arguments not found in '{fullTypeName}'.");
                        continue;
                    }

                    // Convert string args to method parameter types
                    ParameterInfo[] paramInfos = method.GetParameters();
                    object[] finalArgs = new object[argStrings.Length];

                    for (int i = 0; i < argStrings.Length; i++)
                    {
                        finalArgs[i] = Convert.ChangeType(argStrings[i], paramInfos[i].ParameterType);
                    }

                    method.Invoke(null, finalArgs);
                    break; // Success
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
