#pragma once
#include "CSharpInterop.h"

class ICSharpBackend 
{
public:
	virtual ~ICSharpBackend() = default;

	virtual void Initialize() = 0;
	virtual MethodReturnValue RunMethod(const char* methodNamespace) = 0;
	bool m_initialized = false;
};