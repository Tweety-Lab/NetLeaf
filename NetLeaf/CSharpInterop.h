#pragma once
#include <cstdint>

enum class ReturnType : int
{
    None = 0,
    String = 1,
    Float = 2,
    UInt = 3,
    Int = 4
};

struct MethodReturnValue
{
    const char* StringResult;
    float FloatResult;
    uint32_t UIntResult;
    int32_t IntResult;
    ReturnType Type;
};