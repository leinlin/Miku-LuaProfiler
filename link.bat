@echo off

mklink /D "%~dp0\XLua\Assets\LuaProfiler" "%~dp0\LuaProfiler"
mklink /D "%~dp0\SLua\Assets\LuaProfiler" "%~dp0\LuaProfiler"
mklink /D "%~dp0\ToLua\Assets\LuaProfiler" "%~dp0\LuaProfiler"

pause