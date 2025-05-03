using System;
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
                Console.WriteLine("RunMethod invoked from native code");

                // Log methodNamespace
                string methodNamespaceStr = null;
                if (methodNamespace != IntPtr.Zero)
                {
                    // On Windows, this should be a wide string (UTF-16)
                    // On other platforms, this should be UTF-8
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        methodNamespaceStr = Marshal.PtrToStringUni(methodNamespace);
                    }
                    else
                    {
                        methodNamespaceStr = Marshal.PtrToStringUTF8(methodNamespace);
                    }
                    Console.WriteLine($"Method namespace: {methodNamespaceStr}");
                }
                else
                {
                    Console.WriteLine("Method namespace pointer is null");
                }

                // Extract loaded assemblies
                string[] assemblies = new string[assembliesCount];
                if (loadedAssemblies != IntPtr.Zero && assembliesCount > 0)
                {
                    for (int i = 0; i < assembliesCount; i++)
                    {
                        // Get the pointer to the assembly path string
                        IntPtr assemblyPathPtr = Marshal.ReadIntPtr(loadedAssemblies, i * IntPtr.Size);
                        if (assemblyPathPtr != IntPtr.Zero)
                        {
                            // Convert the pointer to a string
                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                            {
                                assemblies[i] = Marshal.PtrToStringAnsi(assemblyPathPtr);
                            }
                            else
                            {
                                assemblies[i] = Marshal.PtrToStringUTF8(assemblyPathPtr);
                            }
                            Console.WriteLine($"Loaded Assembly {i}: {assemblies[i]}");
                        }
                        else
                        {
                            Console.WriteLine($"Assembly pointer {i} is null");
                            assemblies[i] = null;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No assemblies provided or pointer is null");
                }

                // Execute additional logic based on methodNamespace
                if (!string.IsNullOrEmpty(methodNamespaceStr))
                {
                    Console.WriteLine($"Executing method in namespace: {methodNamespaceStr}");
                }

                Console.WriteLine("RunMethod completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RunMethod: {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}