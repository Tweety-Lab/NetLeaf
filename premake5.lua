-- Setup Workspace
workspace "NetLeafWorkspace"
    configurations { "Debug", "Release" }
    architecture "x64"

-- C++ project (NetLeaf)
project "NetLeaf"
    kind "SharedLib"
    language "C++"
    location "NetLeaf"
    targetdir ("%{wks.location}/build/bin")
    objdir ("%{wks.location}/obj/%{prj.name}")

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
            -- Copy headers to shared include dir
            "{COPY} %{prj.location}/*.h %{wks.location}/build/include/",
            "{COPY} %{prj.location}/include/*.h %{wks.location}/build/include/",

            -- Copy .NET dlls
            "{COPY} %{prj.location}/bin/*.dll %{wks.location}/build/bin/"
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
    targetdir ("%{wks.location}/Tests/bin")
    objdir ("%{wks.location}/obj/%{prj.name}")

    files {
        "Tests/CPPTests/**.cpp",
        "Tests/CPPTests/**.h"
    }

    removefiles {
        "Tests/CPPTests/thirdparty/**.h"
    }

    includedirs {
        "Tests/CPPTests/thirdparty/doctest",
        "NetLeaf/include"
    }

    libdirs {
        "%{wks.location}/build/bin/%{cfg.buildcfg}-%{cfg.system}-%{cfg.architecture}"
    }

    links {
        "NetLeaf",
        "NetLeaf.Bridge",
        "CSharpTests"
    }

    filter "system:windows"
        systemversion "latest"
        defines { "WIN32", "_WINDOWS" }

        postbuildcommands {
            -- Copy DLLs from NetLeaf
            "{COPY} %{wks.location}/build/bin/*.dll %{cfg.targetdir}",
            -- Copy C# Jsons from NetLeaf
            "{COPY} %{wks.location}/build/bin/*.json %{cfg.targetdir}"
        }

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
