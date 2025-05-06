#define CATCH_CONFIG_MAIN
#include <catch.hpp>
#include "../../NetLeaf/NetLeaf.h"
#include "../../NetLeaf/DotNetBackend.h"

// DotNetHost Initialization
TEST_CASE(".NET Initialization", "[dotnet]") {
    auto* backend = new DotNetBackend();
    NetLeaf::LoadCSharpBackend(backend);

    REQUIRE(backend->IsInitialized() == true);

    delete backend;
}