-- Setup Workspace
workspace "NetLeafWorkspace"
    configurations { "Debug", "Release" }
    architecture "x64"

-- C++ project (NetLeaf)
project "NetLeaf"
    kind "SharedLib"
    language "C++"
    location "NetLeaf"
    targetdir ("build/bin/")
    objdir ("obj/")

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

        postbuildcommands {
            -- Copy headers to include directory
            "{COPY} %{prj.location}/*.h %{wks.location}/build/include/",
            "{COPY} %{prj.location}/include/*.h %{wks.location}/build/include/",
        }
        
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
        location "Tests/CPPTests"
        targetdir "Tests/bin/%{cfg.buildcfg}-%{cfg.system}-%{cfg.architecture}"

        includedirs {
            "Tests/CPPTests/thirdparty/doctest",
            "NetLeaf/include"
        }

        libdirs {
            "build/lib"
        }

        files {
            "Tests/CPPTests/**.cpp",
            "Tests/CPPTests/**.h"
        }

        removefiles {
            "Tests/CPPTests/thirdparty/**.h"
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