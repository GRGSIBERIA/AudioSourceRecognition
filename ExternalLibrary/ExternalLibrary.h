// ExternalLibrary.h : 標準のシステム インクルード ファイル用のインクルード ファイル、
// または、プロジェクト専用のインクルード ファイル。

#pragma once

#ifdef _MSC_VER
#pragma warning(disable:4251)
#endif

#ifdef _WIN32
#ifdef DLL_BODY
#define DLL_EXPORT  __declspec(dllexport)
#else
#define DLL_EXPORT  __declspec(dllimport)
#endif
#else
#define DLL_EXPORT
#endif

#define DLL_API extern "C" __declspec(dllexport)

#ifdef _MSC_VER
#include <intrin.h>
#else
#include <x86intrin.h>
#endif

#define EL_ARCHITECTURE_AVX 8
#define EL_ARCHITECTURE_SSE 4

// 定義上はAVXで計算する
#define EL_ARCHITECTURE EL_ARCHITECTURE_AVX

#include <memory>

/// <summary>
/// 複素数型
/// </summary>
__declspec(align(16)) struct complex
{
	float re;
	float im;
};

static std::unique_ptr<float> windowed = nullptr;
