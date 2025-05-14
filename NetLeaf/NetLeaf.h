#pragma once

#include <vector>
#include "ICSharpBackend.h"
#include "NetLeafInstance.h"

#ifdef NETLEAF_EXPORTS
#define NETLEAF_API __declspec(dllexport)  // Export for DLL
#else
#define NETLEAF_API __declspec(dllimport)  // Import for users of the DLL
#endif

class NETLEAF_API NetLeaf
{
public:
	static void LoadCSharpBackend(ICSharpBackend* backend);
	static ICSharpBackend* GetLoadedBackend();
	static void LoadAssembly(const char* assemblyPath);
	static void UnloadAssembly(const char* assemblyPath);
	static MethodReturnValue RunCSharpMethod(const char* methodNamespace);
	static NetLeafInstance* CreateInstance(const char* typeNamespace);
private:
	static ICSharpBackend* loadedBackend;

};

