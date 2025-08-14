@echo off

set NDK=%ANDROID_NDK_ROOT%
set NDKABI=25
set NDKVER=%NDK%/toolchains/aarch64-linux-android-4.9
set NDKP=%NDKVER%/prebuilt/windows-x86_64/bin/aarch64-linux-android-
set STRIP=%NDKP%"strip.exe"

rmdir /s /q build_android
mkdir build_android && cd build_android
cmake  -G "Unix Makefiles" -DANDROID_ABI=arm64-v8a -DCMAKE_TOOLCHAIN_FILE=%NDK%/build/cmake/android.toolchain.cmake -DANDROID_NATIVE_API_LEVEL=android-10 ../
cd ..
cmake --build build_android --config Release -j8

mkdir Plugins\Android\libs\arm64-v8a
copy build_android\libmiku_hook.so Plugins\Android\libs\arm64-v8a\libmiku_hook.so

rem Android/ARM, armeabi-v7a
set NDKVER=%NDK%/toolchains/arm-linux-androideabi-4.9
set NDKP=%NDKVER%/prebuilt/windows-x86_64/bin/arm-linux-androideabi-
set STRIP=%NDKP%"strip.exe"

rmdir /s /q build_android
mkdir build_android && cd build_android

cmake  -G "Unix Makefiles" -DANDROID_ARM_MODE=arm -DANDROID_ABI=armeabi-v7a -DCMAKE_TOOLCHAIN_FILE=%NDK%/build/cmake/android.toolchain.cmake -DANDROID_NATIVE_API_LEVEL=android-10 ../
cd ..
cmake --build build_android --config Release -j8

mkdir Plugins\Android\libs\armeabi-v7a
copy build_android\libmiku_hook.so Plugins\Android\libs\armeabi-v7a\libmiku_hook.so


rem Android/Intel, x86
set NDKVER=%NDK%/toolchains/x86-4.9
set NDKP=%NDKVER%/prebuilt/windows-x86_64/bin/i686-linux-android-
set STRIP=%NDKP%"strip.exe"

rmdir /s /q build_android
mkdir build_android && cd build_android

cmake  -G "Unix Makefiles" -DANDROID_ABI=x86 -DCMAKE_TOOLCHAIN_FILE=%NDK%/build/cmake/android.toolchain.cmake -DANDROID_NATIVE_API_LEVEL=android-10 ../
cd ..
cmake --build build_android --config Release -j8

mkdir Plugins\Android\libs\x86
copy build_android\libmiku_hook.so Plugins\Android\libs\x86\libmiku_hook.so