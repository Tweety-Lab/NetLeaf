using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Linq;

namespace NetLeaf.Bridge;

public static class Methods
{
    [UnmanagedCallersOnly(EntryPoint = "RunMethod")]
    public static void RunMethod(IntPtr methodNamespace, IntPtr resultPtr)
    {
        MethodReturnValue result = new()
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
                Console.Error.WriteLine("[RunMethod] methodNamespace was null.");
                WriteResult(resultPtr, result);
                return;
            }

            string? methodNamespaceStr = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Marshal.PtrToStringUni(methodNamespace) : Marshal.PtrToStringUTF8(methodNamespace);

            if (methodNamespaceStr == null)
            {
                Console.Error.WriteLine("[RunMethod] methodNamespaceStr was null.");
                WriteResult(resultPtr, result);
                return;
            }


            if (!TryParseMethodNamespace(methodNamespaceStr, out string fullMethodPath, out string[] argStrings))
            {
                Console.Error.WriteLine($"[RunMethod] Failed to parse method namespace: {methodNamespaceStr}");
                WriteResult(resultPtr, result);
                return;
            }

            // Use unified method resolution
            MethodInfo? method = Assemblies.FindMethod(fullMethodPath, argStrings.Length);
            if (method != null)
            {
                InvokeMethod(method, argStrings, resultPtr, result);
                return;
            }

            Console.Error.WriteLine($"[RunMethod] Method not found: {fullMethodPath} with {argStrings.Length} args.");
            WriteResult(resultPtr, result);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[RunMethod] Exception: {ex}");
            WriteResult(resultPtr, result);
        }
    }

    private static bool TryParseMethodNamespace(string methodNamespaceStr, out string fullMethodPath, out string[] argStrings)
    {
        fullMethodPath = null!;
        argStrings = Array.Empty<string>();

        int parenStart = methodNamespaceStr.IndexOf('(');
        int parenEnd = methodNamespaceStr.LastIndexOf(')');
        if (parenStart == -1 || parenEnd == -1 || parenEnd <= parenStart)
            return false;

        fullMethodPath = methodNamespaceStr[..parenStart];
        string argsStr = methodNamespaceStr[(parenStart + 1)..parenEnd];

        string[] pathParts = fullMethodPath.Split('.');
        if (pathParts.Length < 3)
            return false;

        string @namespace = string.Join(".", pathParts.Take(pathParts.Length - 2));
        string className = pathParts[^2];
        string methodName = pathParts[^1];
        fullMethodPath = $"{@namespace}.{className}.{methodName}";

        argStrings = argsStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return true;
    }

    private static void InvokeMethod(MethodInfo method, string[] argStrings, IntPtr resultPtr, MethodReturnValue result)
    {
        ParameterInfo[] paramInfos = method.GetParameters();
        object[] finalArgs = new object[argStrings.Length];

        for (int i = 0; i < argStrings.Length; i++)
        {
            finalArgs[i] = Convert.ChangeType(argStrings[i], paramInfos[i].ParameterType);
        }

        object? returnValue = method.Invoke(null, finalArgs);

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
                result.StringResult = Marshal.StringToHGlobalAnsi(returnValue.ToString() ?? string.Empty);
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
