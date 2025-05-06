using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTests
{
    public class ConstructingClass
    {
        public ConstructingClass()
        {
            // Write to a file
            File.WriteAllText("TESTCLASSINSTANCE.txt", "ConstructingClass's Constructor Ran successfully!");
        }
    }

    public class MethodsClass
    {
        public void InstanceMethod()
        {
            Console.WriteLine("InstanceMethod() Ran successfully!");
        }
    }
}
