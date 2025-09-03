rm -rf build_osx
mkdir -p build_osx && cd build_osx
cmake -GXcode ../

cd ..

cmake --build build_osx --config Release
mkdir -p Plugins/miku_hook.bundle/Contents/MacOS/

cp build_osx/Release/miku_hook.bundle/Contents/MacOS/miku_hook Plugins/miku_hook.bundle/Contents/MacOS/miku_hook
