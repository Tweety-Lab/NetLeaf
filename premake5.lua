-- Setup Workspace
workspace "NetLeafWorkspace"
    configurations { "Debug", "Release" }
    architecture "x64"

-- Set the location for the output build
outputdir = "%{cfg.buildcfg}-%{cfg.system}-%{cfg.architecture}"

-- C++ project (NetLeaf)
project "NetLeaf"
    kind "SharedLib"
    language "C++"
    location "NetLeaf"
    targetdir ("bin/" .. outputdir)
    objdir ("obj/" .. outputdir)

    files {
        "NetLeaf/**.cpp",
        "NetLeaf/**.h"
    }

    includedirs {
        "NetLeaf/include",
    }

    libdirs {
        "NetLeaf/lib"
    }

    links {
        "nethost.lib"
    }

    filter "system:windows"
        systemversion "latest"
        defines { "WIN32", "_WINDOWS" }
        
    filter "system:linux"
        defines { "LINUX" }

    filter "configurations:Debug"
        defines { "DEBUG" }
        symbols "On"

    filter "configurations:Release"
        defines { "NDEBUG" }
        optimize "On"

-- C# project (NetLeaf.Bridge)
externalproject "NetLeaf.Bridge"
    location "NetLeaf.Bridge"
    kind "SharedLib"
    language "C#"

-- Unit Tests
group "Tests"
    -- C++ Unit Tests
    project "CPPTests"
        kind "ConsoleApp"
        language "C++"
        targetdir "Tests/bin/%{cfg.buildcfg}-%{cfg.system}-%{cfg.architecture}"

        includedirs {
            "Tests/CPPTests/thirdparty/catch2",
            "NetLeaf/include"
        }

        files {
            "Tests/CPPTests/**.cpp",
            "Tests/CPPTests/**.h"
        }

        -- Link against NetLeaf
        links {
            "NetLeaf",
            "NetLeaf.Bridge",
            "CSharpTests"
        }

        filter "system:windows"
            systemversion "latest"
            defines { "WIN32", "_WINDOWS" }

        filter "system:linux"
            defines { "LINUX" }

        filter "configurations:Debug"
            defines { "DEBUG" }
            symbols "On"

        filter "configurations:Release"
            defines { "NDEBUG" }
            optimize "On"
        
    -- C# Unit Tests
    externalproject "CSharpTests"
        location "Tests/CSharpTests"
        kind "SharedLib"
        language "C#"