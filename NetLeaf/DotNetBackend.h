#pragma once
#include <string>
#include <unordered_map>
#include <iostream>
#include <nethost.h>
#include <coreclr_delegates.h>
#include <hostfxr.h>

#include "NetLeafAPI.h"
#include "ICSharpBackend.h"

class NETLEAF_API DotNetBackend : public ICSharpBackend
{
private:
    // HostFxr function pointers
    hostfxr_initialize_for_runtime_config_fn m_initForConfig = nullptr;
    hostfxr_get_runtime_delegate_fn m_getDelegate = nullptr;
    hostfxr_close_fn m_close = nullptr;

    // CoreCLR hosting members
    hostfxr_handle m_hostContext = nullptr;
    load_assembly_and_get_function_pointer_fn m_loadAssemblyFunc = nullptr;

    // Bridge Assembly management
    std::string m_assemblyPath = "NetLeaf.Bridge.dll";
    std::string m_runtimeConfigPath = "NetLeaf.Bridge.runtimeconfig.json";

    // Initialization
    bool m_initialized = false;

    // Helper methods
    void* LoadLib(const char_t* path);
    void* GetExport(void* hModule, const char* name);
    bool LoadHostFxr();
    bool InitializeHost();
    load_assembly_and_get_function_pointer_fn GetLoadAssemblyFunction(const char_t* configPath);

    // Helper to get the directory of the running library
    std::string GetLibraryDirectory();

public:
    // ICSharpBackend
    void Initialize() override;
    MethodReturnValue RunMethod(const char* methodNamespace) override;

    void LogError(const std::string& message);
    void LogInfo(const std::string& message);

    bool IsInitialized() const { return m_initialized; }
};