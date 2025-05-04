#include "pch.h"

#define DOTNETBACKEND_EXPORTS
#include "DotNetBackend.h"
#include "NetLeaf.h"

void* DotNetBackend::LoadLib(const char_t* path)
{
#ifdef _WIN32
    HMODULE h = ::LoadLibraryW(path);
    if (h == nullptr) {
        LogError("Failed to load library");
    }
#else
    void* h = dlopen(path, RTLD_LAZY | RTLD_LOCAL);
    if (h == nullptr) {
        LogError("Failed to load library: " + std::string(path) + ", error: " + std::string(dlerror()));
    }
#endif
    return h;
}

void* DotNetBackend::GetExport(void* hModule, const char* name)
{
    if (hModule == nullptr) {
        LogError("Cannot get export from null module");
        return nullptr;
    }

#ifdef _WIN32
    void* f = ::GetProcAddress(static_cast<HMODULE>(hModule), name);
    if (f == nullptr) {
        LogError("Failed to get function export: " + std::string(name) + ", error: " + std::to_string(GetLastError()));
    }
#else
    void* f = dlsym(hModule, name);
    if (f == nullptr) {
        LogError("Failed to get function export: " + std::string(name) + ", error: " + std::string(dlerror()));
    }
#endif
    return f;
}

bool DotNetBackend::LoadHostFxr()
{
    get_hostfxr_parameters params{ sizeof(get_hostfxr_parameters), nullptr, nullptr };
    char_t buffer[MAX_PATH];
    size_t buffer_size = sizeof(buffer) / sizeof(char_t);

    int rc = get_hostfxr_path(buffer, &buffer_size, &params);
    if (rc != 0)
    {
        LogError("Failed to get hostfxr path: 0x" + std::to_string(rc));
        return false;
    }

    void* lib = LoadLib(buffer);
    if (!lib)
    {
        return false; // LoadLib already printed an error
    }

    m_initForConfig = (hostfxr_initialize_for_runtime_config_fn)GetExport(lib, "hostfxr_initialize_for_runtime_config");
    m_getDelegate = (hostfxr_get_runtime_delegate_fn)GetExport(lib, "hostfxr_get_runtime_delegate");
    m_close = (hostfxr_close_fn)GetExport(lib, "hostfxr_close");

    if (!m_initForConfig || !m_getDelegate || !m_close)
    {
        LogError("Failed to get hostfxr exports");
        return false;
    }

    return true;
}

load_assembly_and_get_function_pointer_fn DotNetBackend::GetLoadAssemblyFunction(const char_t* configPath)
{
    if (!m_getDelegate || !m_initForConfig) {
        LogError("Host functions not initialized");
        return nullptr;
    }

    hostfxr_handle hostContext = nullptr;
    int rc = m_initForConfig(configPath, nullptr, &hostContext);
    if (rc != 0 || !hostContext)
    {
        LogError("Failed to initialize host: 0x" + std::to_string(rc));
        return nullptr;
    }

    // Store the host context for later use
    m_hostContext = hostContext;

    // Get the load assembly function
    load_assembly_and_get_function_pointer_fn loadAssemblyFunc = nullptr;
    rc = m_getDelegate(
        hostContext,
        hdt_load_assembly_and_get_function_pointer,
        (void**)&loadAssemblyFunc);

    if (rc != 0 || !loadAssemblyFunc)
    {
        LogError("Failed to get load assembly function: 0x" + std::to_string(rc));
        m_close(hostContext);
        m_hostContext = nullptr;
        return nullptr;
    }

    return loadAssemblyFunc;
}

bool DotNetBackend::InitializeHost()
{
#if defined(_WIN32)
    std::wstring wideConfigPath(m_runtimeConfigPath.begin(), m_runtimeConfigPath.end());  // Convert std::string to std::wstring
    const wchar_t* config_path_ptr = wideConfigPath.c_str();  // Use wide char string for Windows
#else
    const char* config_path_ptr = m_runtimeConfigPath.c_str();  // Use narrow char string for non-Windows platforms
#endif

    // Get the load assembly function
    m_loadAssemblyFunc = GetLoadAssemblyFunction(config_path_ptr);
    if (m_loadAssemblyFunc == nullptr)
    {
        LogError("Failed to initialize host");
        return false;
    }

    return true;
}

std::string DotNetBackend::GetLibraryDirectory()
{
    // Buffer to hold the full path to the library
    char path[MAX_PATH];

    // Get the full path of the currently running library
    if (GetModuleFileNameA(NULL, path, MAX_PATH) == 0)
    {
        LogError("Failed to get the library path");
        return "";
    }

    // Find the last backslash in the path
    std::string fullPath(path);
    size_t lastSlash = fullPath.find_last_of("\\/");
    if (lastSlash != std::string::npos)
    {
        // Return the directory part of the full path
        return fullPath.substr(0, lastSlash + 1);
    }

    // If no directory found, return empty string
    return "";
}

void DotNetBackend::Initialize()
{
    if (!m_initialized)
    {
        // Get the directory where the library is running from
        std::string libDirectory = GetLibraryDirectory();
        if (libDirectory.empty())
        {
            LogError("Failed to retrieve library directory");
            return;
        }

        // Set the assembly and runtime config paths based on the library directory
        m_assemblyPath = libDirectory + "NetLeaf.Bridge.dll";
        m_runtimeConfigPath = libDirectory + "NetLeaf.Bridge.runtimeconfig.json";

        if (!LoadHostFxr())
        {
            LogError("Failed to load hostfxr");
            return;
        }

        if (!InitializeHost())
        {
            LogError("Failed to initialize host");
            return;
        }

        m_initialized = true;
        LogInfo("DotNetBackend initialized successfully");
    }
    else
    {
        LogInfo("DotNetBackend already initialized");
    }
}

MethodReturnValue DotNetBackend::RunMethod(const char* methodNamespace)
{
    if (!m_initialized)
    {
        LogError("DotNetBackend not initialized");
        return MethodReturnValue{};
    }

    if (m_loadAssemblyFunc == nullptr)
    {
        LogError("LoadAssembly function pointer is null");
        return MethodReturnValue{};
    }

    // Derive the assembly name (without extension)
    size_t lastSlash = m_assemblyPath.find_last_of("/\\");
    std::string fileName = (lastSlash != std::string::npos)
        ? m_assemblyPath.substr(lastSlash + 1)
        : m_assemblyPath;

    size_t dotPos = fileName.find_last_of('.');
    std::string assemblyName = (dotPos != std::string::npos)
        ? fileName.substr(0, dotPos)
        : fileName;

    std::string fullTypeName = "NetLeaf.Bridge.Methods, " + assemblyName;

#ifdef _WIN32
    std::wstring assemblyPathW(m_assemblyPath.begin(), m_assemblyPath.end());
    std::wstring typeNameW(fullTypeName.begin(), fullTypeName.end());
    std::wstring methodNameW(L"RunMethod");

    const char_t* assemblyPath = assemblyPathW.c_str();
    const char_t* typeName = typeNameW.c_str();
    const char_t* methodName = methodNameW.c_str();
#else
    const char_t* assemblyPath = m_assemblyPath.c_str();
    const char_t* typeName = fullTypeName.c_str();
    const char_t* methodName = "RunMethod";
#endif

    // Convert methodNamespace to proper format based on platform
#ifdef _WIN32
    std::wstring methodNamespaceW;
    if (methodNamespace != nullptr)
    {
        int size_needed = MultiByteToWideChar(CP_UTF8, 0, methodNamespace, -1, NULL, 0);
        if (size_needed > 0)
        {
            methodNamespaceW.resize(size_needed);
            MultiByteToWideChar(CP_UTF8, 0, methodNamespace, -1, &methodNamespaceW[0], size_needed);
        }
    }
    void* methodNamespacePtr = methodNamespace ? (void*)methodNamespaceW.c_str() : nullptr;
#else
    void* methodNamespacePtr = (void*)methodNamespace;
#endif

    // Create an array of loaded assembly paths
    std::vector<const char*> loadedAssemblies = NetLeaf::GetLoadedAssemblyPaths();
    size_t assembliesCount = loadedAssemblies.size();
    void* loadedAssembliesPtr = malloc(assembliesCount * sizeof(const char*));
    if (loadedAssembliesPtr == nullptr)
    {
        LogError("Failed to allocate memory for loaded assemblies");
        return MethodReturnValue{};
    }

    for (size_t i = 0; i < assembliesCount; ++i)
    {
        *((const char**)loadedAssembliesPtr + i) = loadedAssemblies[i];
    }

    // Load method pointer
    void* functionPtr = nullptr;
    int rc = m_loadAssemblyFunc(
        assemblyPath,
        typeName,
        methodName,
        UNMANAGEDCALLERSONLY_METHOD,
        nullptr,
        &functionPtr
    );

    if (rc != 0 || functionPtr == nullptr)
    {
        LogError("Failed to get method dispatcher from assembly. HRESULT: 0x" + std::to_string(rc));
        free(loadedAssembliesPtr);
        return MethodReturnValue{};
    }

    typedef void(CORECLR_DELEGATE_CALLTYPE* MethodInvoker)(void*, void*, int, void*);
    MethodInvoker invoker = reinterpret_cast<MethodInvoker>(functionPtr);

    MethodReturnValue returnValue = {}; // Zero-init

    try
    {
        // Call the managed method
        invoker(methodNamespacePtr, loadedAssembliesPtr, (int)assembliesCount, &returnValue);
    }
    catch (...)
    {
        LogError("Exception occurred during managed method invocation");
    }

    free(loadedAssembliesPtr);

    // Return result
    return returnValue;
}

void DotNetBackend::LogError(const std::string& message)
{
    std::cerr << "[ERROR] " << message << std::endl;
}

void DotNetBackend::LogInfo(const std::string& message)
{
    std::cout << "[INFO] " << message << std::endl;
}