#include <stdlib.h>
#include <stdint.h>
#include <stdbool.h>
#include <mikuhooker.h>
#include <inlineHook.h>
#ifdef MIKU_HOOKER_WIN
#include <windows.h>
#elif MIKU_HOOKER_ANDROID
#include <sys/mman.h>
#include <dlfcn.h>
#endif

#ifndef PAGE_SIZE
#define PAGE_SIZE 4096
#endif

#define CLEAR_BIT0(addr)    (addr & 0xFFFFFFFE)
#define PAGE_START(addr)    (~(PAGE_SIZE - 1) & (addr))

MIKU_HOOKER_API void* miku_hooker_jit_alloc(void* addr, int32_t length)
{
#ifdef MIKU_HOOKER_WIN
    return VirtualAlloc(addr, (size_t)length, MEM_RESERVE | MEM_COMMIT, PAGE_EXECUTE_READWRITE);
#elif MIKU_HOOKER_ANDROID
    return mmap(addr, length, PROT_EXEC | PROT_READ | PROT_WRITE, MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);
#else
    return NULL;
#endif
}

MIKU_HOOKER_API bool miku_hooker_protect(void* addr, int32_t length, int prot)
{
#ifdef MIKU_HOOKER_WIN
    DWORD pflOldProtect;
    return VirtualProtect(addr, (size_t)length, PAGE_EXECUTE_READWRITE, &pflOldProtect);
#elif MIKU_HOOKER_ANDROID
    uint32_t target_addr = (uint32_t)addr;
    mprotect((void *) PAGE_START(CLEAR_BIT0(target_addr)), PAGE_SIZE * 2, prot);
#else
    return false;
#endif
}

MIKU_HOOKER_API bool miku_hooker_free_library(const char* filename)
{
#ifdef MIKU_HOOKER_WIN
    void* handle = LoadLibrary(filename);
    return FreeLibrary(handle);
#elif MIKU_HOOKER_ANDROID
    void* handle = dlopen(filename, RTLD_NOW);
    return dlclose(handle);
#else
    return NULL;
#endif
}

MIKU_HOOKER_API void* miku_hooker_get_library_addr(const char* filename, const char* symbol)
{
#ifdef MIKU_HOOKER_WIN
    return LoadLibrary(filename);
#elif MIKU_HOOKER_ANDROID
    return dlopen(filename, RTLD_NOW);
#else
    return NULL;
#endif
}

MIKU_HOOKER_API void* miku_hooker_get_address(const char* filename, const char* symbol)
{
#ifdef MIKU_HOOKER_WIN
    void* handle = LoadLibrary(filename);
    return GetProcAddress(handle, symbol);
#elif MIKU_HOOKER_ANDROID
    void* handle = dlopen(filename, RTLD_NOW);
    return dlsym(handle, symbol);
#else
    return NULL;
#endif
}

MIKU_HOOKER_API bool miku_hooker_free(void* addr, int32_t length)
{
#ifdef MIKU_HOOKER_WIN
    return VirtualFree(addr, (size_t)length, MEM_RELEASE);
#elif MIKU_HOOKER_ANDROID
    return munmap(addr, (size_t)length) == 0;
#else
    return false;
#endif
}

MIKU_HOOKER_API bool miku_android_arm_hook(uint32_t oldFun, uint32_t newFun, uint32_t* proxyFun)
{
#if MIKU_HOOKER_ANDROID
    return registerInlineHook(oldFun, newFun, proxyFun) == ELE7EN_OK;
#else
    return false;
#endif
}

MIKU_HOOKER_API bool miku_android_arm_unhook(uint32_t oldFun)
{
#if MIKU_HOOKER_ANDROID
    return false;
#else
    return false;
#endif
}