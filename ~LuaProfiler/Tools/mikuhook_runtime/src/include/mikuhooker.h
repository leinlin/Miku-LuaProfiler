#ifndef MIKU_HOOKER
#define MIKU_HOOKER

#ifdef MIKU_HOOKER_WIN
#define MIKU_HOOKER_API __attribute__((dllexport))
#else
#define MIKU_HOOKER_API extern
#endif

#endif