using System.Runtime.InteropServices;

namespace NetLeaf.Bridge
{
    public static class Utility
    {
        [UnmanagedCallersOnly]
        public static void RunMethod(IntPtr methodNamespace, IntPtr loadedAssemblies, int assembliesCount)
        {
            // Convert string pointer to a C# string
            string methodNamespaceStr = Marshal.PtrToStringAnsi(methodNamespace);

            // Convert unmanaged array of string pointers to a managed string array
            string[] assemblies = new string[assembliesCount];
            for (int i = 0; i < assembliesCount; i++)
            {
                IntPtr currentAssemblyPtr = Marshal.ReadIntPtr(loadedAssemblies, i * IntPtr.Size);
                assemblies[i] = Marshal.PtrToStringAnsi(currentAssemblyPtr);
            }

            // Debug
            Console.WriteLine($"Method: {methodNamespaceStr}");
            foreach (var assembly in assemblies)
            {
                Console.WriteLine($"Loaded Assembly: {assembly}");
            }
        }
    }
}
