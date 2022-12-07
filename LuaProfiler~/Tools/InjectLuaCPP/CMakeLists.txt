cmake_minimum_required(VERSION 3.0.2)
set(CMAKE_INSTALL_PREFIX install)

project(luainject)

FILE(GLOB_RECURSE SRC_LIST "${PROJECT_SOURCE_DIR}/src/*.cpp")
INCLUDE_DIRECTORIES(include)

add_executable(luainject ${HELLO_WIN_SRC_LIST})

