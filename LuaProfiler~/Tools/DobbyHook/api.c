#include <dobby.h>
#include <sys/mman.h>
#include <unistd.h>     // for sysconf

extern int atri_Hook(void *address, void *replace_call, void **origin_call)
{
	int page_size = sysconf(_SC_PAGE_SIZE);
	uintptr_t addr_start = (uintptr_t)address & ~(page_size - 1);
	
    mprotect(addr_start, page_size, PROT_READ | PROT_WRITE | PROT_EXEC);
    int ret = DobbyHook(address, replace_call, origin_call);
    return ret;
}

extern int atri_UnHook(void *address)
{
    return DobbyDestroy(address);
}