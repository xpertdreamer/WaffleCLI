cmake_minimum_required(VERSION 3.10)
project(simple_calc)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

add_executable(simple_calc simple_calc.cpp)

# For Windows
if(WIN32)
    set_target_properties(simple_calc PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin"
    )
endif()

# For Linux/macOS
if(UNIX)
    set_target_properties(simple_calc PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin"
    )
endif()