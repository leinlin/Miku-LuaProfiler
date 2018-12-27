#!/bin/bash
rm -rf $PWD/SLuaClient/Assets/Plugins/LuaProfilerClient
ln -s $PWD/LuaProfiler/LuaProfilerClient $PWD/SLuaClient/Assets/Plugins/LuaProfilerClient

rm -rf $PWD/XLuaClient/Assets/LuaProfilerClient
ln -s $PWD/LuaProfiler/LuaProfilerClient $PWD/XLuaClient/Assets/LuaProfilerClient

rm -rf $PWD/ToLuaClient/Assets/LuaProfilerClient
ln -s $PWD/LuaProfiler/LuaProfilerClient $PWD/ToLuaClient/Assets/LuaProfilerClient

rm -rf $PWD/Server/Assets/LuaProfilerServer
ln -s $PWD/LuaProfiler/LuaProfilerServer $PWD/Server/Assets/LuaProfilerServer