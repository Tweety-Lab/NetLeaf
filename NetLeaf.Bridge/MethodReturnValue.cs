using System.Runtime.InteropServices;

namespace NetLeaf.Bridge;

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
