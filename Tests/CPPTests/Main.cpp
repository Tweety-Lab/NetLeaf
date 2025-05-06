#define DOCTEST_CONFIG_IMPLEMENT
#include "doctest.h"
#include "../../NetLeaf/NetLeaf.h"
#include "../../NetLeaf/DotNetBackend.h"

int main(int argc, char** argv) {
    // Run initial setup

    // Load .NET backend for testing
    std::cout << "Initializing DotNet Backend for Tests...\n";
    NetLeaf::LoadCSharpBackend(new DotNetBackend());

    // Load C# Test assembly
    NetLeaf::LoadAssembly("CSharpTests.dll");

    doctest::Context context;

    context.applyCommandLine(argc, argv);

    int res = context.run(); // Run tests

    return res;
}