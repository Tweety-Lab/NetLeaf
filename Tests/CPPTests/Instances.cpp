#include "doctest.h"
#include "../../NetLeaf/NetLeaf.h"

#include <fstream>
#include <string>

TEST_CASE("Instance Successfuly Runs Constructor")
{
	const char* filePath = "TESTCLASSINSTANCE.txt";

	NetLeaf::CreateInstance("CSharpTests.ConstructingClass");

    // Read the output from the file created by TestClass's constructor
    std::ifstream file(filePath);
    REQUIRE(file.is_open());

    std::string fileContent((std::istreambuf_iterator<char>(file)), std::istreambuf_iterator<char>());
    file.close();

    // Check that the expected output is in the file
    REQUIRE(fileContent.find("ConstructingClass's Constructor Ran successfully!") != std::string::npos);

    // Delete the file
    std::remove(filePath);
}

TEST_CASE("Instance Methods Successfully Run")
{
    NetLeafInstance* instance = NetLeaf::CreateInstance("CSharpTests.MethodsClass");
    instance->RunMethod("InstanceMethod()");
}