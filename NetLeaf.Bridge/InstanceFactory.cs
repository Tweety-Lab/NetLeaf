using System;
using System.Collections.Generic;
using System.Reflection;

namespace NetLeaf.Bridge
{
    /// <summary>
    /// Management of class instances.
    /// </summary>
    public static class InstanceFactory
    {
        // Map C++ IDs to Class Instances
        // TODO: Check if we can replace a uint handle with IntPtr to C++ Class?
        private static Dictionary<uint, object> instanceMap = new Dictionary<uint, object>();
        private static uint currentID = 0;

        // Create an instance from its namespace
        public static uint CreateInstance(string typeNamespace)
        {
            uint id = currentID++;

            try
            {
                Type type = Assemblies.FindTypeInLoadedAssemblies(typeNamespace);
                if (type == null)
                {
                    Console.WriteLine($"[CreateInstance Error] Type '{typeNamespace}' not found in loaded assemblies.");
                    return 0;
                }

                object instance = Activator.CreateInstance(type);
                instanceMap[id] = instance;
                return id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CreateInstance Error] {ex.Message}");
                return 0;
            }
        }

        // Delete an instance from its ID/Handle
        public static void DeleteInstance(uint id)
        {
            instanceMap.Remove(id);
        }

        // Run an instance's method from its ID/Handle and the method name
        public static void RunInstanceMethod(uint id, string methodCall)
        {
            // Get the instance
            if (!instanceMap.TryGetValue(id, out object instance))
                return;

            // Parse the method call into method name and arguments
            Type type = instance.GetType();
            string methodName = ParseMethodName(methodCall, out string[] argStrings);

            if (methodName == null)
                return;

            // Find the method matching the name and the number of arguments
            var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (method == null)
                return;

            // Convert arguments to the correct types
            object[] args = ConvertArguments(method, argStrings);
            method.Invoke(instance, args);
        }

        private static string ParseMethodName(string methodCall, out string[] args)
        {
            int parenStart = methodCall.IndexOf('(');
            int parenEnd = methodCall.LastIndexOf(')');

            if (parenStart == -1 || parenEnd == -1 || parenEnd <= parenStart)
            {
                args = null;
                return null;
            }

            string methodName = methodCall.Substring(0, parenStart).Trim();
            string argsStr = methodCall.Substring(parenStart + 1, parenEnd - parenStart - 1).Trim();

            args = argsStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                          .Select(arg => arg.Trim())
                          .ToArray();

            return methodName;
        }

        private static object[] ConvertArguments(MethodInfo method, string[] argStrings)
        {
            ParameterInfo[] paramInfos = method.GetParameters();
            object[] finalArgs = new object[argStrings.Length];

            for (int i = 0; i < argStrings.Length; i++)
            {
                finalArgs[i] = Convert.ChangeType(argStrings[i], paramInfos[i].ParameterType);
            }

            return finalArgs;
        }
    }
}
