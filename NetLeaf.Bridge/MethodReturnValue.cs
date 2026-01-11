using System.Runtime.InteropServices;

namespace NetLeaf.Bridge;

/// <summary>
/// Struct used for safetly passing a methods return value to unmanaged code.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct MethodReturnValue
{
    public IntPtr StringResult;
    public float FloatResult;
    public uint UIntResult;
    public int IntResult;

    public ReturnType Type;
}

public enum ReturnType : int
{
    None = 0,
    String = 1,
    Float = 2,
    UInt = 3,
    Int = 4
}
