#!/bin/bash
# 64 Bit Version

gcc -DMIKU_HOOKER_WIN -m64 -O2 -std=gnu99 -shared \
 miku_profiler.c \
  -o Plugins/x86_64/miku_profiler.dll \
 -Isrc/include \
 -Wl, -static-libgcc -static-libstdc++
 
strip Plugins/x86_64/miku_profiler.dll --strip-unneeded