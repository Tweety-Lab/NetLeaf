#include "doctest.h"
#include "../../NetLeaf/NetLeaf.h"

#include <fstream>
#include <string>

TEST_CASE("CSharpTests/Methods/TestStaticMethod writes expected output to file") {
    const char* filePath = "STATICVOIDMETHODSTEST.txt";

    // Call the C# static method which writes to test.txt
    NetLeaf::RunCSharpMethod("CSharpTests.Methods.TestStaticMethod()");

    // Read the output from the file
    std::ifstream file(filePath);
    REQUIRE(file.is_open());

    std::string fileContent((std::istreambuf_iterator<char>(file)), std::istreambuf_iterator<char>());
    file.close();

    // Check that the expected output is in the file
    REQUIRE(fileContent.find("TestStaticMethod() Ran successfully!") != std::string::npos);

    // Delete the file
	std::remove(filePath);
}

TEST_CASE("CSharpTests/Methods") {
    SUBCASE("TestStaticIntMethod Returns expected integer value") {
        MethodReturnValue result = NetLeaf::RunCSharpMethod("CSharpTests.Methods.TestStaticIntMethod()");
        REQUIRE(result.Type == ReturnType::Int);
        REQUIRE(result.IntResult == 32);
    }

    SUBCASE("TestStaticStringMethod Returns expected string value") {
        MethodReturnValue result = NetLeaf::RunCSharpMethod("CSharpTests.Methods.TestStaticStringMethod()");
        REQUIRE(result.Type == ReturnType::String);
        REQUIRE(result.StringResult != nullptr);

        // TODO: Compare Strings
    }

    SUBCASE("TestStaticUIntMethod Returns expected uint value") {
        MethodReturnValue result = NetLeaf::RunCSharpMethod("CSharpTests.Methods.TestStaticUIntMethod()");
        REQUIRE(result.Type == ReturnType::UInt);
        REQUIRE(result.UIntResult == 32);
    }
}