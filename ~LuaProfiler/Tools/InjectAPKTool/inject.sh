#!/bin/bash

APK_NAME=$PWD/$1
DLL_PATH='assets/bin/Data/Managed/Assembly-CSharp.dll'
PROFILER_DLL_PATH='assets/bin/Data/Managed/LuaProfilerDLL.dll'

$PWD/aapt.exe r $APK_NAME META-INF/CERT.RSA
$PWD/aapt.exe r $APK_NAME META-INF/CERT.SF
$PWD/aapt.exe r $APK_NAME META-INF/MANIFEST.MF

echo "unzip dll"

$PWD/InjectToLua.exe --unzip $APK_NAME

echo "inject dll"

$PWD/InjectToLua.exe -l -m $DLL_PATH

echo "replace dll"
$PWD/aapt.exe r $APK_NAME $DLL_PATH
$PWD/aapt.exe a $APK_NAME $DLL_PATH
$PWD/aapt.exe a $APK_NAME $PROFILER_DLL_PATH

echo "re signed"
jarsigner -verbose -keystore $2 -signedjar injected.apk $APK_NAME $3 -keypass $4 -storepass $4

rm -rf assets
