#pragma once

#include <vector>
#include "ICSharpBackend.h"

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
	static MethodReturnValue RunCSharpMethod(const char* methodNamespace);
private:
	static ICSharpBackend* loadedBackend;

};

