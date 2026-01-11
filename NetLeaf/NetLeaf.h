#pragma once

#include <vector>
#include "ICSharpBackend.h"
#include "NetLeafInstance.h"
#include "NetLeafAPI.h"

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

