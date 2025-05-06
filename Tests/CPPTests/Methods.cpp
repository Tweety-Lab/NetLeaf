#include "doctest.h"
#include "../../NetLeaf/NetLeaf.h"

#include <fstream>
#include <string>

TEST_CASE("Static Void Method") {
    // Call the C# static method which writes to test.txt
    NetLeaf::RunCSharpMethod("CSharpTests.Methods.TestStaticMethod()");

    // Read the output from the file
    std::ifstream file("STATICVOIDMETHODSTEST.txt");
    REQUIRE(file.is_open());

    std::string fileContent((std::istreambuf_iterator<char>(file)), std::istreambuf_iterator<char>());
    file.close();

    // Check that the expected output is in the file
    REQUIRE(fileContent.find("TestStaticMethod() Ran successfully!") != std::string::npos);

    // Delete the file
	std::remove("STATICVOIDMETHODSTEST.txt");
}