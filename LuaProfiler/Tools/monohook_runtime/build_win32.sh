#!/bin/bash
# 64 Bit Version

gcc -DMONO_HOOKER_WIN -m32 -O2 -std=gnu99 -shared \
 src/source/monohooker.c \
  -o Plugins/x86/monohooker.dll \
 -Isrc/include \
 -Wl, -static-libgcc -static-libstdc++
 
strip Plugins/x86/monohooker.dll --strip-unneeded