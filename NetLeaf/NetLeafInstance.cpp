#include "NetLeafInstance.h"
#include "NetLeaf.h"
#include <string>


NetLeafInstance::NetLeafInstance(const char* typeNamespace)
{
    std::string methodCall = "NetLeaf.Bridge.InstanceFactory.CreateInstance(" + std::string(typeNamespace) + ")";
    instanceID = NetLeaf::GetLoadedBackend()->RunMethod(methodCall.c_str()).UIntResult;
}

NetLeafInstance::~NetLeafInstance()
{
	std::string methodCall = "NetLeaf.Bridge.InstanceFactory.DeleteInstance(" + std::to_string(instanceID) + ")";
	NetLeaf::GetLoadedBackend()->RunMethod(methodCall.c_str());
}

MethodReturnValue NetLeafInstance::RunMethod(const char* methodNamespace)
{
	std::string methodCall = "NetLeaf.Bridge.InstanceFactory.RunInstanceMethod(" + std::to_string(instanceID) + ", " + std::string(methodNamespace) + ")";
	return NetLeaf::GetLoadedBackend()->RunMethod(methodCall.c_str());
}