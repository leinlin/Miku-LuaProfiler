#include <dobby.h>

#if _MSC_VER
#else
#include <sys/mman.h>
#include <unistd.h>     // for sysconf
#include <dlfcn.h>
#endif

#if defined(BUILD_AS_DLL)
#if defined(_MSC_VER)
#define ATRI_API		__declspec(dllexport)
#else
#define ATRI_API		extern
#endif
#else
#define ATRI_API		extern
#endif

ATRI_API int miku_Hook(void *address, void *replace_call, void **origin_call)
{
#if _MSC_VER
#else
    size_t page_size = sysconf(_SC_PAGE_SIZE);
    size_t addr_start = (size_t)address & ~(page_size - 1);

    mprotect((void*)addr_start, page_size, PROT_READ | PROT_WRITE | PROT_EXEC);
#endif
    int ret = DobbyHook(address, replace_call, origin_call);
    return ret;
}

ATRI_API int miku_UnHook(void *address)
{
#if _MSC_VER
    return 0;
#else
    return DobbyDestroy(address);
#endif
}

ATRI_API void* miku_dlopen(const char* path, int mode)
{
#if _MSC_VER
    return NULL;
#else
    return dlopen(path, mode);
#endif
}

ATRI_API void* miku_get_dlopen_ptr()
{
#if _MSC_VER
    return NULL;
#else
    return (void*)dlopen;
#endif
}

ATRI_API void* miku_dlsymbol(void* handle, const char* symbol)
{
#if _MSC_VER
    return NULL;
#else
    return dlsym(handle, symbol);
#endif
}