# CMakeList.txt : ExternalLibrary の CMake プロジェクト。ソースを含めて、次を定義します:
# プロジェクト専用ロジックはこちらです。
#
cmake_minimum_required (VERSION 3.8)

project ("ExternalLibrary")

# ソースをこのプロジェクトの実行可能ファイルに追加します。
add_executable (ExternalLibrary "ExternalLibrary.cpp" "ExternalLibrary.h" "Misc.cpp" "WindowFunction.cpp")

# TODO: テストを追加し、必要な場合は、ターゲットをインストールします。
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin")
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin")