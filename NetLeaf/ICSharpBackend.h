#pragma once

class ICSharpBackend 
{
public:
	virtual ~ICSharpBackend() = default;

	virtual void Initialize() = 0;
	virtual void RunMethod(const char* methodNamespace) = 0;
};