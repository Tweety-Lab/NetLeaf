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
        public static void RunMethod(IntPtr methodNamespace, IntPtr loadedAssemblies, int assembliesCount, IntPtr resultPtr)
        {
            MethodReturnValue result = new MethodReturnValue
            {
                Type = ReturnType.None,
                StringResult = IntPtr.Zero,
                FloatResult = 0f,
                IntResult = 0,
                UIntResult = 0
            };

            try
            {
                // Validate and decode method namespace
                if (methodNamespace == IntPtr.Zero)
                {
                    WriteResult(resultPtr, result);
                    return;
                }

                string methodNamespaceStr = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? Marshal.PtrToStringUni(methodNamespace)
                    : Marshal.PtrToStringUTF8(methodNamespace);

                // Validate and extract assembly paths
                if (loadedAssemblies == IntPtr.Zero || assembliesCount == 0)
                {
                    WriteResult(resultPtr, result);
                    return;
                }

                string[] assemblies = new string[assembliesCount];
                for (int i = 0; i < assembliesCount; i++)
                {
                    IntPtr ptr = Marshal.ReadIntPtr(loadedAssemblies, i * IntPtr.Size);
                    assemblies[i] = ptr != IntPtr.Zero
                        ? (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                            ? Marshal.PtrToStringAnsi(ptr)
                            : Marshal.PtrToStringUTF8(ptr))
                        : null;
                }

                // Parse: Namespace.Class.Method(args...)
                int parenStart = methodNamespaceStr.IndexOf('(');
                int parenEnd = methodNamespaceStr.LastIndexOf(')');
                if (parenStart == -1 || parenEnd == -1 || parenEnd <= parenStart)
                {
                    WriteResult(resultPtr, result);
                    return;
                }

                string fullMethodPath = methodNamespaceStr[..parenStart];
                string argsStr = methodNamespaceStr[(parenStart + 1)..parenEnd];
                string[] pathParts = fullMethodPath.Split('.');
                if (pathParts.Length < 3)
                {
                    WriteResult(resultPtr, result);
                    return;
                }

                string @namespace = string.Join(".", pathParts.Take(pathParts.Length - 2));
                string className = pathParts[^2];
                string methodName = pathParts[^1];
                string fullTypeName = $"{@namespace}.{className}";

                string[] argStrings = argsStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                foreach (string assemblyName in assemblies)
                {
                    if (string.IsNullOrEmpty(assemblyName)) continue;

                    string fullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), assemblyName);
                    Assembly assembly = Assembly.LoadFrom(fullPath);
                    Type type = assembly.GetType(fullTypeName);
                    if (type == null) continue;

                    MethodInfo method = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                        .FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == argStrings.Length);

                    if (method == null) continue;

                    ParameterInfo[] paramInfos = method.GetParameters();
                    object[] finalArgs = new object[argStrings.Length];
                    for (int i = 0; i < argStrings.Length; i++)
                    {
                        finalArgs[i] = Convert.ChangeType(argStrings[i], paramInfos[i].ParameterType);
                    }

                    object returnValue = method.Invoke(null, finalArgs);

                    // Handle typed return values
                    if (returnValue != null)
                    {
                        Type returnType = method.ReturnType;

                        if (returnType == typeof(string))
                        {
                            string str = (string)returnValue;
                            result.Type = ReturnType.String;
                            result.StringResult = Marshal.StringToHGlobalAnsi(str);
                        }
                        else if (returnType == typeof(float))
                        {
                            result.Type = ReturnType.Float;
                            result.FloatResult = (float)returnValue;
                        }
                        else if (returnType == typeof(int))
                        {
                            result.Type = ReturnType.Int;
                            result.IntResult = (int)returnValue;
                        }
                        else if (returnType == typeof(uint))
                        {
                            result.Type = ReturnType.UInt;
                            result.UIntResult = (uint)returnValue;
                        }
                        else
                        {
                            // Unsupported return type: fallback to ToString()
                            string str = returnValue.ToString();
                            result.Type = ReturnType.String;
                            result.StringResult = Marshal.StringToHGlobalAnsi(str);
                        }
                    }

                    WriteResult(resultPtr, result);
                    return; // Success
                }

                // If we get here, nothing was found
                WriteResult(resultPtr, result);
            }
            catch
            {
                // On failure, return empty result
                WriteResult(resultPtr, result);
            }
        }

        private static void WriteResult(IntPtr resultPtr, MethodReturnValue result)
        {
            if (resultPtr != IntPtr.Zero)
            {
                Marshal.StructureToPtr(result, resultPtr, false);
            }
        }
    }
}
