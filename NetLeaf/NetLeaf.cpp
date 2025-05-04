#define NETLEAF_EXPORTS

#include "pch.h"
#include "NetLeaf.h"
#include <iostream>

// Static member definition
std::vector<const char*> NetLeaf::loadedAssemblyPaths;
ICSharpBackend* NetLeaf::loadedBackend = nullptr;


void NetLeaf::LoadAssembly(const char* assemblyPath) 
{
	// Add assembly path to the list of loaded assemblies
	loadedAssemblyPaths.push_back(assemblyPath);
}

void NetLeaf::LoadCSharpBackend(ICSharpBackend* backend) 
{
	// Store the backend instance
	loadedBackend = backend;

	// Initialize the backend
	loadedBackend->Initialize();
}

void NetLeaf::RunCSharpMethod(const char* methodNamespace)
{
	// Call the method on the loaded backend
    if (loadedBackend)
    {
        loadedBackend->RunMethod(methodNamespace);
	}
	else 
	{
		std::cerr << "Error: No CSharp backend loaded. Unable to run method: " << methodNamespace << std::endl;
	}
}

// Return the vector of loaded assembly paths
std::vector<const char*> NetLeaf::GetLoadedAssemblyPaths()
{
	return loadedAssemblyPaths;
}