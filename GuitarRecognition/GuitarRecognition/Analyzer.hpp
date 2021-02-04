#pragma once
#include <otfft.h>
#include <vector>
#include <omp.h>
#include <Siv3D.hpp>
#include <intrin.h>
#include <exception>
#include "TinyASIO/TinyASIO.hpp"
using OTFFT::complex_t;
using OTFFT::simd_malloc;
using OTFFT::simd_free;

class Analyzer
{
	template <class T>
	class SIMDAllocator : public std::allocator<T>
	{
	public:
		SIMDAllocator() : std::allocator<T>() {}

		template <class U>
		SIMDAllocator(const SIMDAllocator<U>&) : std::allocator<T> {}

		T* allocate(size_t n)
		{
			return reinterpret_cast<T*>(simd_malloc(sizeof(T) * n))
		}

		void deallocate(T* p, size_t n)
		{
			static_cast<void>(n);
			simd_free(p);
		}
	};

	std::vector<complex_t*> outputs;
	std::vector<double*> results;
	std::vector<std::vector<double>> resultCapsules;
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
		resultCapsules.resize(stride);
		for (int i = 0; i < stride; ++i)
		{
			allocation(N >> i);
			results.push_back((double*)simd_malloc(N >> i * sizeof(double)));
			resultCapsules[i].resize(N >> i);
		}
		workspace = (complex_t*)simd_malloc(sizeof(complex_t) * N);
	}

public:
	/**
	 * @param N FFTのサンプル点数
	 * @param stride N点を起点として窓幅をN>>strideずつ縮小しながらスペクトルを求める
	 * @param slide 平均スペクトルを求める回数
	 * @note
	 * N = 16384, stride = 4, slide = 4の場合
	 * strideは、{fft(16384), fft(8192), fft(4096), fft(2048)}の系列を作る
	 * slideは、strideの平均スペクトルを求める回数
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

private:
	void computeAveragePowerSpectrums(const size_t st, const size_t sl)
	{
		const double invSlide = 1.0 / slide;
		const double invs[2] = { invSlide, invSlide };

		complex_t* rv = outputs[st * stride + sl];
		auto& average = results[st];

		// 答えの配列をゼロで初期化する
		memset(&results[st], 0.0, sizeof(complex_t) * (N >> st));

		// intrinsicな感じでパワースペクトルを求める
		for (size_t ni = 0; ni < (N >> st); ++ni)
		{
			// 意訳すると以下の処理をしたいだけ
			// double d = 0.0;
			//d = rv[ni].Im * rv[ni].Im + rv[ni].Re * rv[ni].Re;
			//d = Math::Sqrt(d);
			//d = d * d / inv;
			//average[ni] += d;

			__m128d d = _mm_load_pd((double*)&rv[ni]);
			__m128d inv = _mm_load_pd((double*)invs);

			// 二乗平方根を取る
			d = _mm_sqrt_pd(_mm_dp_pd(d, d, 0b11111111));
			d = _mm_mul_pd(_mm_mul_pd(d, d), inv);
			//A * (x + y + z) = Ax + Ay + Azのような計算に展開する
			
			// 加算平均を取る
			__m128d avg = _mm_load_pd((double*)&average[ni]);
			avg = _mm_add_pd(avg, d);

			_mm_storel_pd(&average[ni], avg);
		}
	}

public:
	void fft(const std::vector<asio::SampleType>& samples)
	{
		// intをdoubleに変換する
#pragma omp paralell for
		for (int i = 0; i < N; ++i)
		{
			workspace[i] = complex_t(samples[i], 0.0);
		}
		// workspace.size() = timeLength * samplingFrequency

		// 入力データを一斉にロードする
#pragma omp paralell for
		for (size_t st = 0; st < stride; ++st)
		{
			for (size_t sl = 0; sl < slide; ++sl)
			{
				const size_t movesize = sizeof(complex_t) * (N >> st);
				const size_t workindex = N - N >> sl;
				memmove_s(
					outputs[st * (size_t)stride + sl],
					movesize,
					&workspace[workindex],
					movesize
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

		// 平均スペクトルを計算する
#pragma omp parallel for
		for (size_t st = 0; st < stride; ++st)
		{
			for (size_t sl = 0; sl < slide; ++sl)
			{
				computeAveragePowerSpectrums(st, sl);
			}
		}

		// 安全に確保したメモリに書き込む
#pragma omp parallel for
		for (size_t st = 0; st < stride; ++st)
		{
			const size_t size = resultCapsules[st].size() * sizeof(double);
			memmove_s(
				&resultCapsules[st][0],
				size,
				results[st],
				size);
		}
	}

	const int getStride() const { return stride; }

	/**
	 * @param i 取得する処理結果、iが増加するとsizeは半分に減少する
	 * @returns フーリエ変換の結果
	 */
	const std::vector<double>& result(const size_t i) const 
	{ 
		if (resultCapsules.size() <= i)
			throw std::invalid_argument("given i over than stride.");

		return resultCapsules[i]; 
	}
};