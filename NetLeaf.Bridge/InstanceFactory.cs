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

            // TODO: Use reflection to create a new instance of typeNamespace

            // Add this instance to the instanceMap
            instanceMap[id] = "Placeholder Object";

            // Return the ID of the created instance
            return id;
        }

        // Delete an instance from its ID/Handle
        public static void DeleteInstance(uint id)
        {
            // Remove the instance from the dictionary
            instanceMap.Remove(id);
        }

        // Run an instance's method from its ID/Handle and the method name
        public static void RunInstanceMethod(uint id, string methodName)
        {
            // Get the instance
            object instance = instanceMap[id];

            if (instance == null)
                return;

            Type type = instance.GetType(); // Get the instances type
            var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic); // Get the method

            // Invoke the method
            method?.Invoke(instance, null);
        }
    }
}
