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
        public static void RunMethod(IntPtr methodNamespace, IntPtr resultPtr)
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
                if (methodNamespace == IntPtr.Zero)
                {
                    WriteResult(resultPtr, result);
                    return;
                }

                string methodNamespaceStr = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? Marshal.PtrToStringUni(methodNamespace)
                    : Marshal.PtrToStringUTF8(methodNamespace);

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

                // First, try to find the method in the current assembly
                MethodInfo method = FindMethodInAssembly(Assembly.GetExecutingAssembly(), fullTypeName, methodName, argStrings.Length);
                if (method != null)
                {
                    InvokeMethod(method, argStrings, resultPtr, result);
                    return;
                }

                // Then, search through all loaded assemblies
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    method = FindMethodInAssembly(assembly, fullTypeName, methodName, argStrings.Length);
                    if (method != null)
                    {
                        InvokeMethod(method, argStrings, resultPtr, result);
                        return;  // Return after successfully invoking the method
                    }
                }

                // If nothing is found, return with the default result
                WriteResult(resultPtr, result);
            }
            catch
            {
                WriteResult(resultPtr, result);
            }
        }

        private static MethodInfo FindMethodInAssembly(Assembly assembly, string fullTypeName, string methodName, int paramCount)
        {
            Type type = assembly.GetType(fullTypeName);
            if (type == null) return null;

            return type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                       .FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == paramCount);
        }

        private static void InvokeMethod(MethodInfo method, string[] argStrings, IntPtr resultPtr, MethodReturnValue result)
        {
            ParameterInfo[] paramInfos = method.GetParameters();
            object[] finalArgs = new object[argStrings.Length];
            for (int i = 0; i < argStrings.Length; i++)
            {
                finalArgs[i] = Convert.ChangeType(argStrings[i], paramInfos[i].ParameterType);
            }

            object returnValue = method.Invoke(null, finalArgs);

            if (returnValue != null)
            {
                Type returnType = method.ReturnType;

                if (returnType == typeof(string))
                {
                    result.Type = ReturnType.String;
                    result.StringResult = Marshal.StringToHGlobalAnsi((string)returnValue);
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
                    result.Type = ReturnType.String;
                    result.StringResult = Marshal.StringToHGlobalAnsi(returnValue.ToString());
                }
            }

            WriteResult(resultPtr, result);
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
