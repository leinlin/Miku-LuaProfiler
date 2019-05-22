#!/bin/bash
# 64 Bit Version

gcc -DMONO_HOOKER_WIN -m32 -O2 -std=gnu99 -shared \
 miku_profiler.c \
  -o Plugins/x86/miku_profiler.dll \
 -Isrc/include \
 -Wl, -static-libgcc -static-libstdc++
 
strip Plugins/x86/miku_profiler.dll --strip-unneeded