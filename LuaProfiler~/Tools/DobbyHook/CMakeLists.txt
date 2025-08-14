cmake_minimum_required(VERSION 3.18.1)

set(MODULE_NAME miku_hook)

if (NOT DEFINED MODULE_NAME)
    message(FATAL_ERROR "MODULE_NAME is not set")
else ()
    project(${MODULE_NAME})
endif ()

message("Build type: ${CMAKE_BUILD_TYPE}")

set(CMAKE_CXX_STANDARD 20)

set(LINKER_FLAGS "-ffixed-x18 -Wl,--hash-style=both")
set(C_FLAGS "-Werror=format -fdata-sections -ffunction-sections")
set(CXX_FLAGS "${CXX_FLAGS} -fno-exceptions -fno-rtti")

if (NOT CMAKE_BUILD_TYPE STREQUAL "Debug")
    set(C_FLAGS "${C_FLAGS} -O2")
    set(LINKER_FLAGS "${LINKER_FLAGS} -Wl,-exclude-libs,ALL -Wl,--gc-sections -Wl,--strip-all")
else ()
    set(C_FLAGS "${C_FLAGS} -O0")
endif ()

set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} ${C_FLAGS}")
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} ${C_FLAGS} ${CXX_FLAGS}")

# set(CMAKE_SHARED_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} ${LINKER_FLAGS}")
# set(CMAKE_MODULE_LINKER_FLAGS "${CMAKE_MODULE_LINKER_FLAGS} ${LINKER_FLAGS}")

set(DobbyHome dobby)
include_directories(
        ${DobbyHome}/include
)

macro(SET_OPTION option value)
    set(${option} ${value} CACHE INTERNAL "" FORCE)
endmacro()
SET_OPTION(DOBBY_DEBUG OFF)
SET_OPTION(DOBBY_GENERATE_SHARED OFF)
add_subdirectory(${DobbyHome} Dobby)

add_library(${MODULE_NAME} SHARED
        api.c)

target_link_libraries(${MODULE_NAME} log dl dobby)

if (NOT CMAKE_BUILD_TYPE STREQUAL "Debug")
    add_custom_command(TARGET ${MODULE_NAME} POST_BUILD
          COMMAND ${CMAKE_STRIP} --strip-unneeded --remove-section=.comment "$<TARGET_FILE:${MODULE_NAME}>")
endif ()
