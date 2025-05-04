using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NetLeaf.Bridge
{
    // Struct for passing a methods return value back to C++
    [StructLayout(LayoutKind.Sequential)]
    public struct MethodReturnValue
    {
        public IntPtr StringResult; // Pointer to marshaled string
        public float FloatResult;
        public uint UIntResult;
        public int IntResult;

        public ReturnType Type; // Indicate the type of the return value
    }

    public enum ReturnType : int
    {
        None = 0,
        String = 1,
        Float = 2,
        UInt = 3,
        Int = 4
    }
}
