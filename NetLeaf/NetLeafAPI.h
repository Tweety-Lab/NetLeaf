#pragma once

#if defined(_WIN32)

#if defined(NETLEAF_BUILD)
#define NETLEAF_API __declspec(dllexport)
#else
#define NETLEAF_API __declspec(dllimport)
#endif

#else
#define NETLEAF_API __attribute__((visibility("default")))
#endif