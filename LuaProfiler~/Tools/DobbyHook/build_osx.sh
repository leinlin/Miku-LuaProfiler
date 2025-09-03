rm -rf build_osx
mkdir -p build_osx && cd build_osx
cmake -GXcode ../

cd ..

cmake --build build_osx --config Release
mkdir -p Plugins/atri_hook.bundle/Contents/MacOS/

cp build_osx/Release/atri_hook.bundle/Contents/MacOS/atri_hook Plugins/atri_hook.bundle/Contents/MacOS/atri_hook
