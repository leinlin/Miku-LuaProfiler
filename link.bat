@echo off

rd "%~dp0\SLuaClient\Assets\Plugins\LuaProfilerClient"
mklink /D "%~dp0\SLuaClient\Assets\Plugins\LuaProfilerClient" "%~dp0\LuaProfiler\LuaProfilerClient"

rd "%~dp0\XLuaClient\Assets\LuaProfilerClient"
mklink /D "%~dp0\XLuaClient\Assets\LuaProfilerClient" "%~dp0\LuaProfiler\LuaProfilerClient"

rd "%~dp0\Server\Assets\LuaProfilerServer"
mklink /D "%~dp0\Server\Assets\LuaProfilerServer" "%~dp0\LuaProfiler\LuaProfilerServer"

pause