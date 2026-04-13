cmake_minimum_required(VERSION 3.20)

set(PROJECT_NAME "baremetal")

list(APPEND CMAKE_MODULE_PATH "/home/chenzch/Projects/NXP/CMakeTemplateS32DS/cmake")

set(BUILD_TYPE "Debug" CACHE STRING "Build type: Debug or Release")

add_definitions(
    -DINIT_STDBY_RAM
    -DARMCM7_SP
)

include_directories(
    ${CMAKE_SOURCE_DIR}/include
)

aux_source_directory(${CMAKE_SOURCE_DIR}/src APPLICATION_SOURCES)

set(LINKER_SCRIPT "${CMAKE_SOURCE_DIR}/Project_Settings/Linker_Files/S32K312_flash.ld")

set(ADDITIONAL_LIBRARIES "")

set(ADDITIONAL_LINKER_FLAGS "-nostartfiles -nodefaultlibs -nostdlib")

include(source/s32ds)
include(toolchain/gcc14)

include(template)

project(${PROJECT_NAME} C ASM)
