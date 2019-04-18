#ifndef MONO_HOOKER
#define MONO_HOOKER

#ifdef MONO_HOOKER_WIN
#define MONO_HOOKER_API __attribute__((dllexport))
#else
#define MONO_HOOKER_API extern
#endif

#endif