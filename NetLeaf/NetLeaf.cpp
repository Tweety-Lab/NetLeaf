#define NETLEAF_EXPORTS

#include "pch.h"
#include "NetLeaf.h"
#include <iostream>

// Static member definition
ICSharpBackend* NetLeaf::loadedBackend = nullptr;


void NetLeaf::LoadAssembly(const char* assemblyPath)
{
    if (!loadedBackend) return;

    // Pass the assembly path without surrounding quotes
    std::string methodCall = "NetLeaf.Bridge.Assemblies.LoadAssembly(" + std::string(assemblyPath) + ")";
    loadedBackend->RunMethod(methodCall.c_str());
}

void NetLeaf::UnloadAssembly(const char* assemblyPath) 
{
	if (!loadedBackend) return;

	// Pass the assembly path without surrounding quotes
	std::string methodCall = "NetLeaf.Bridge.Assemblies.UnloadAssembly(" + std::string(assemblyPath) + ")";
	loadedBackend->RunMethod(methodCall.c_str());
}

void NetLeaf::LoadCSharpBackend(ICSharpBackend* backend) 
{
	// Store the backend instance
	loadedBackend = backend;

	// Initialize the backend
	loadedBackend->Initialize();
}

ICSharpBackend* NetLeaf::GetLoadedBackend() 
{
	return loadedBackend;
}

MethodReturnValue NetLeaf::RunCSharpMethod(const char* methodNamespace)
{
	// Call the method on the loaded backend
    if (loadedBackend)
    {
        return loadedBackend->RunMethod(methodNamespace);
	}
	else 
	{
		std::cerr << "Error: No CSharp backend loaded. Unable to run method: " << methodNamespace << std::endl;
		return MethodReturnValue{};
	}
}

NetLeafInstance* NetLeaf::CreateInstance(const char* typeNamespace) 
{
	return new NetLeafInstance(typeNamespace);
}