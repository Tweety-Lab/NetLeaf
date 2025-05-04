#pragma once
#include "CSharpInterop.h"

#ifdef NETLEAF_EXPORTS
#define NETLEAF_API __declspec(dllexport)  // Export for DLL
#else
#define NETLEAF_API __declspec(dllimport)  // Import for users of the DLL
#endif

class NETLEAF_API NetLeafInstance
{
public:
	NetLeafInstance(const char* typeNamespace);
	~NetLeafInstance();
	MethodReturnValue RunMethod(const char* methodName);
private:
	uint32_t instanceID;
};

