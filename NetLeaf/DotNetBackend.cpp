#include "pch.h"
#include "DotNetBackend.h"


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

void DotNetBackend::Initialize()
{
    if (!m_initialized)
    {
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

void DotNetBackend::RunMethod(const char* methodNamespace)
{
    if (!m_initialized)
    {
        LogError("DotNetBackend not initialized");
        return;
    }

    if (m_loadAssemblyFunc == nullptr)
    {
        LogError("LoadAssembly function pointer is null");
        return;
    }

    // Attempt to load assembly and run method
    LogInfo("Running method: " + std::string(methodNamespace));

    // TODO: Actual code to invoke method
}

void DotNetBackend::LogError(const std::string& message)
{
    std::cerr << "[ERROR] " << message << std::endl;
}

void DotNetBackend::LogInfo(const std::string& message)
{
    std::cout << "[INFO] " << message << std::endl;
}
