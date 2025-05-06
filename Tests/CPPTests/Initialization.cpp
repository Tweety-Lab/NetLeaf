#define CATCH_CONFIG_MAIN
#include <catch.hpp>
#include "../../NetLeaf/NetLeaf.h"
#include "../../NetLeaf/DotNetBackend.h"

// DotNetHost Initialization
TEST_CASE(".NET Initialization", "[dotnet]") {
    NetLeaf::LoadCSharpBackend(new DotNetBackend());
    REQUIRE(NetLeaf::GetLoadedBackend()->m_initialized == true);
    delete NetLeaf::GetLoadedBackend();
}