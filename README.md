# NetLeaf
*A Simplified way to run C# from native C++ Apps.*

NetLeaf allows simple and fast .NET C# Hosting from C++ Code through a handful of easy to understand methods.

## Features

| Feature            | Supported |
|--------------------|-----------|
| Class Instances     | ✔️         |
| Method Running     | ✔️         |
| Method Arguments   | ✔️         |
| Method Return Values | ✔️       |

NetLeaf utilises the .NET Runtime to support .NET 9.0.0 by default.

## Getting Started
Once you have NetLeaf installed and `NetLeaf.h` included in your file, you can get started with just a few calls.

**Load NetLeaf with a .NET Runtime backend:**
```C++
#include "NetLeaf/DotNetBackend.h"
NetLeaf::LoadCSharpBackend(new DotNetBackend());
```

**Load a C# Assembly:**
```C++
NetLeaf::LoadAssembly("AssemblyName.dll");
```

**Run a Static C# Method:**
```C++
NetLeaf::RunCSharpMethod("Namespace.ClassName.MethodName()");
```

**Create an Instance of a C# Class:**
```C++
NetLeaf::CreateInstance("Namespace.ClassName");
```

**Create an Instance and run one of it's Methods:**
```C++
NetLeafInstance* instance = NetLeaf::CreateInstance("Namespace.ClassName");
instance->RunMethod("MethodName()");
```

## Building

### Prerequisites
Premake5

### Windows
1. Navigate to the Repo
2. Open a Command Prompt
3. Build with the appropriate Premake5 Command (i.e., `premake5 vs2022` for Visual Studio 2022)
