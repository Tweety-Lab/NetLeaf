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
                // Attempt to find the type in all loaded assemblies
                Type type = null;
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = assembly.GetType(typeNamespace);
                    if (type != null)
                        break;
                }

                // If the type is still not found, throw an error
                if (type == null)
                {
                    Console.WriteLine($"[CreateInstance Error] Type '{typeNamespace}' not found in loaded assemblies.");
                    return 0; // Indicate failure to create instance
                }

                // Create an instance of the found type
                object instance = Activator.CreateInstance(type);

                // Add this instance to the instance map
                instanceMap[id] = instance;

                // Return the ID of the created instance
                return id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CreateInstance Error] {ex.Message}");
                return 0; // Return 0 to indicate failure
            }
        }

        // Delete an instance from its ID/Handle
        public static void DeleteInstance(uint id)
        {
            // Remove the instance from the dictionary
            instanceMap.Remove(id);
        }

        // Run an instance's method from its ID/Handle and the method name
        public static void RunInstanceMethod(uint id, string methodCall)
        {
            // Get the instance
            if (!instanceMap.TryGetValue(id, out object instance))
                return;

            Type type = instance.GetType(); // Get the instance's type

            // Parse the method call into method name and arguments
            string methodName = ParseMethodName(methodCall, out string[] argStrings);

            if (methodName == null)
                return;

            // Find the method matching the name and the number of arguments
            var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (method == null)
                return;

            // Convert arguments to the correct types
            object[] args = ConvertArguments(method, argStrings);

            // Invoke the method with the parsed arguments
            method.Invoke(instance, args);
        }

        // TODO: Move these helper methods to a separate class and use them in Methods.cs

        // Helper method to parse the method call string into method name and arguments
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

        // Helper method to convert argument strings to appropriate parameter types
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
