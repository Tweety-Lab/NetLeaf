#pragma once
#include "CSharpInterop.h"
#include "NetLeafAPI.h"

class NETLEAF_API NetLeafInstance
{
public:
	NetLeafInstance(const char* typeNamespace);
	~NetLeafInstance();
	MethodReturnValue RunMethod(const char* methodName);
private:
	uint32_t instanceID;
};

