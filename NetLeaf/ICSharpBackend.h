#pragma once

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

class ICSharpBackend 
{
public:
	virtual ~ICSharpBackend() = default;

	virtual void Initialize() = 0;
	virtual MethodReturnValue RunMethod(const char* methodNamespace) = 0;
};