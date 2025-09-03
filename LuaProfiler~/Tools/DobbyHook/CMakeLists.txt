cmake_minimum_required(VERSION 3.18.1)

set(MODULE_NAME miku_hook)

if (NOT DEFINED MODULE_NAME)
    message(FATAL_ERROR "MODULE_NAME is not set")
else ()
    project(${MODULE_NAME})
endif ()

message("Build type: ${CMAKE_BUILD_TYPE}")

# set(CMAKE_CXX_STANDARD 20)

set(LINKER_FLAGS "-ffixed-x18 -Wl,--hash-style=both")
# set(C_FLAGS "-fdata-sections -ffunction-sections")
# set(CXX_FLAGS "${CXX_FLAGS} -fno-exceptions -fno-rtti")

if (NOT CMAKE_BUILD_TYPE STREQUAL "Debug")
    set(C_FLAGS "${C_FLAGS} -O2")
    set(LINKER_FLAGS "${LINKER_FLAGS} -Wl,-exclude-libs,ALL -Wl,--gc-sections -Wl,--strip-unneeded")
else ()
    set(C_FLAGS "${C_FLAGS} -O0")
endif ()

set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} ${C_FLAGS}")
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} ${C_FLAGS} ${CXX_FLAGS}")

# set(CMAKE_SHARED_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} ${LINKER_FLAGS}")
# set(CMAKE_MODULE_LINKER_FLAGS "${CMAKE_MODULE_LINKER_FLAGS} ${LINKER_FLAGS}")

set(DobbyHome ${CMAKE_SOURCE_DIR}/Dobby)
include_directories(
        ${DobbyHome}/include
)

macro(SET_OPTION option value)
    set(${option} ${value} CACHE INTERNAL "" FORCE)
endmacro()
SET_OPTION(DOBBY_DEBUG OFF)
SET_OPTION(DOBBY_GENERATE_SHARED OFF)


if (APPLE)
    if (IOS)
        message(STATUS "Building for ios")
        set(CMAKE_OSX_ARCHITECTURES "arm64")
        add_library(${MODULE_NAME} STATIC
           api.c
        )
        set_xcode_property (${MODULE_NAME} IPHONEOS_DEPLOYMENT_TARGET "11.0" "all")
        target_link_libraries(${MODULE_NAME} PRIVATE ${DobbyHome}/Apple/ios/libdobby.a c++)
        # 设置 Xcode 属性（可选）
        set_target_properties(${MODULE_NAME} PROPERTIES
            XCODE_ATTRIBUTE_IPHONEOS_DEPLOYMENT_TARGET "11.0"
            XCODE_ATTRIBUTE_ENABLE_BITCODE "NO"  # 若关闭 Bitcode
        )
    else()
        message(STATUS "Building for macOS")

        set(CMAKE_OSX_ARCHITECTURES "x86_64;arm64")
        set(CMAKE_XCODE_ATTRIBUTE_ONLY_ACTIVE_ARCH "NO")
        set(CMAKE_XCODE_ATTRIBUTE_ARCHS "$(ARCHS_STANDARD)")

        add_library(${MODULE_NAME} MODULE
           api.c
        )
        target_link_libraries(${MODULE_NAME} ${DobbyHome}/Apple/macos/libdobby.a c++)
        set_target_properties ( ${MODULE_NAME} PROPERTIES BUNDLE TRUE )
    endif ( )
else()
    add_subdirectory(${DobbyHome} Dobby)
    add_library(${MODULE_NAME} SHARED
       api.c
    )
    target_link_libraries(${MODULE_NAME} dobby "-Wl,-z,max-page-size=16384" "-Wl,-z,common-page-size=16384")
endif ( )

if (MSVC)
   target_compile_definitions (${MODULE_NAME} PRIVATE _CRT_SECURE_NO_WARNINGS _WINSOCK_DEPRECATED_NO_WARNING)
elseif( ANDROID )
   if (NOT CMAKE_BUILD_TYPE STREQUAL "Debug")
        add_custom_command(TARGET ${MODULE_NAME} POST_BUILD
           COMMAND ${CMAKE_STRIP} --strip-unneeded --remove-section=.comment "$<TARGET_FILE:${MODULE_NAME}>")
    endif ()
elseif( IOS )
    add_custom_command(TARGET ${MODULE_NAME} POST_BUILD
        COMMAND libtool -static -o $<TARGET_FILE:${MODULE_NAME}>
                $<TARGET_FILE:${MODULE_NAME}>
                ${DobbyHome}/Apple/ios/libdobby.a
    )
endif ( )


