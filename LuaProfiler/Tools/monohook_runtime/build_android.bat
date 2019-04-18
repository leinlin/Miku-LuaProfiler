@echo off
set CURRENT_FILE=%cd%
set ANDROID_LIB=%cd%\Plugins\Android\libs

rd /S /Q %ANDROID_LIB%
md %ANDROID_LIB%

cd %CURRENT_FILE%/android/jni

call ndk-build clean APP_ABI="armeabi-v7a"
call ndk-build APP_ABI="armeabi-v7a"

xcopy /I /E /Y ..\libs %ANDROID_LIB%

call ndk-build clean APP_ABI="armeabi-v7a"

cd %CURRENT_FILE%

pause