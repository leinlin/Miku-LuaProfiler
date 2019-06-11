#define MIKU_PROFILER_API __attribute__((dllexport))

#include <stdlib.h>
#include <stdbool.h>

bool is_enable_lua = false;
bool is_enable_mono = false;

MIKU_PROFILER_API bool miku_profiler_get_is_enable_lua()
{
	return is_enable_lua;
}

MIKU_PROFILER_API void miku_profiler_set_is_enable_lua(bool value)
{
	is_enable_lua = value;	
}

MIKU_PROFILER_API bool miku_profiler_get_is_enable_mono()
{
	return is_enable_mono;
}

MIKU_PROFILER_API void miku_profiler_set_is_enable_mono(bool value)
{
	is_enable_mono = value;	
}

