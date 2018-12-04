@echo off

mklink /D "%~dp0\XLua\Assets\LuaProfiler\Common" "%~dp0\LuaProfiler\Common"
mklink /D "%~dp0\SLua\Assets\LuaProfiler\Common" "%~dp0\LuaProfiler\Common"
mklink /D "%~dp0\ToLua\Assets\LuaProfiler\Common" "%~dp0\LuaProfiler\Common"
pause