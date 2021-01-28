#pragma once
#include <otfft.h>
#include <vector>
#include <omp.h>
#include "TinyASIO/TinyASIO.hpp"
using OTFFT::complex_t;
using OTFFT::simd_malloc;
using OTFFT::simd_free;

class Analyzer
{
	std::vector<complex_t*> outputs;
	std::vector<OTFFT::FFT> ffts;

	complex_t* workspace = nullptr;

	const int N;
	const int stride;
	const int slide;

	void allocation(const int n)
	{
		for (int i = 0; i < slide; ++i)
		{
			outputs.push_back((complex_t*)simd_malloc(n * sizeof(complex_t)));
			ffts.emplace_back(n);
		}
	}

	void allocateForStrideUnrolling()
	{
		for (int i = 0; i < stride; ++i)
		{
			allocation(N >> i);
		}
		workspace = (complex_t*)simd_malloc(sizeof(complex_t) * N);
	}

public:
	/**
	 * @param N FFTのサンプル点数
	 * @param stride FFTを縮小する数
	 * @param slide FFTをスライドする数
	 * @note
	 * N = 16384, stride = 4, slide = 4の場合
	 * strideは、{fft(16384), fft(8192), fft(4096), fft(2048)}の系列を作る
	 * slideは、それぞれ最大N+N/2を与えられる系列長として、N+N/2だけシフトさせながら平均スペクトルを求める
	 */
	Analyzer(const int N, const int stride, const int slide)
		: N(N), stride(stride), slide(slide)
	{
		allocateForStrideUnrolling();
	}

	/**
	 * 192kHzのサンプリングに合わせて、131,072点のサンプリングをする
	 * 最低でも131072点の音声サンプルが必要
	 */
	Analyzer()
		: N(131072), stride(1), slide(1)
	{
		allocateForStrideUnrolling();
	}

	~Analyzer()
	{
		for (int i = 0; i < outputs.size(); ++i)
			simd_free(outputs[i]);

		simd_free(workspace);
	}

	void fft(const std::vector<asio::SampleType>& samples)
	{
		// intをdoubleに変換する
#pragma omp paralell for
		for (int i = 0; i < N; ++i)
		{
			workspace[i] = complex_t(samples[i], 0.0);
		}

		// 入力データを一斉にロードする
#pragma omp paralell for
		for (size_t st = 0; st < stride; ++st)
		{
			for (size_t sl = 0; sl < slide; ++sl)
			{
				memmove_s(
					outputs[st * (size_t)stride + sl],
					sizeof(complex_t) * (N >> st),
					workspace,
					sizeof(complex_t) * (N >> st)
				);
			}
		}

		// fftを実行する
		for (size_t st = 0; st < stride; ++st)
		{
			for (size_t sl = 0; sl < slide; ++sl)
			{
				ffts[st * stride + sl].fwd(outputs[st * stride + slide]);
			}
		}
			
	}
};