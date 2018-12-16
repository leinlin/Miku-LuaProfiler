@echo off

rd "%~dp0\Client\Assets\LuaProfilerClient"
mklink /D "%~dp0\Client\Assets\LuaProfilerClient" "%~dp0\LuaProfiler\LuaProfilerClient"

rd "%~dp0\Server\Assets\LuaProfilerClient"
mklink /D "%~dp0\Server\Assets\LuaProfilerServer" "%~dp0\LuaProfiler\LuaProfilerServer"

pause