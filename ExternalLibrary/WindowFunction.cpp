#include "ExternalLibrary.h"
#include <memory>
#include <cmath>

static int pvLength = 0;
static int pvFreq = 0;
static double diff = 0;
static double time = 0;
static int mod = 0;

static std::unique_ptr<float> window = nullptr;

const double pi = acos(-1);

void CheckWindowMemory(int, int, void (*)(int, int));
void EL_Windowing(float*, int);

void CheckWindowMemory(int length, int freq, void (*WF)(int, int))
{
	if (pvLength != length || pvFreq != freq)
	{
		pvLength = length;
		pvFreq = freq;

		diff = 1.0 / (double)freq;
		time = diff * (double)length;	// lengthはwaveformの個数
		mod = length % EL_ARCHITECTURE;

		// メモリを初期化
		if (window != nullptr)
			window.reset();
		window = std::make_unique<float>(new float(length));
		windowed = std::make_unique<float>(new float(length));

		WF(length, freq);
	}
}

void EL_Windowing(float* waveform, int length)
{
#if EL_ARCHITECTURE == 8
	for (int i = 0; i < length; i += EL_ARCHITECTURE_AVX)
	{
		__m256 wave = _mm256_load_ps(&waveform[i]);
		__m256 win = _mm256_load_ps(&window.get()[i]);
		__m256 out = _mm256_mul_ps(wave, win);
		_mm256_store_ps(&windowed.get()[i], out);
	}
#elif EL_ARCHITECTURE == 4
	for (int i = 0; i < length; i += EL_ARCHITECTURE_SSE)
	{
		__m128 wave = _mm_load_ps(&waveform[i]);
		__m128 win = _mm_load_ps(&window.get()[i]);
		__m128 out = _mm_mul_ps(wave, win);
		_mm_store_ps(&windowed.get()[i], out);
	}
#endif

	// 余りがある場合は余りの処理を行う
	if (mod > 0)
	{
		for (int i = 0; i < mod; ++i)
		{
			int id = length - mod - 1;
			windowed.get()[id] =
				waveform[id] * window.get()[id];
		}
	}
}

// ハン窓
void EL_Hann(int, int);
void EL_Hann(int length, int spfreq)
{
	for (int i = 0; i < length; ++i)
	{
		window.get()[i] = 0.5 - 0.5 * cos(2.0 * pi * (i * diff / time));
	}
}

// ハミング窓
void EL_Hamming(int, int);
void EL_Hamming(int length, int spfreq)
{
	for (int i = 0; i < length; ++i)
	{
		window.get()[i] = 0.54 - 0.46 * cos(2.0 * pi * (i * diff / time));
	}
}

void EL_UseWindowHann(float* waveform, int length, int spfreq)
{
	CheckWindowMemory(length, spfreq, &EL_Hann);
	EL_Windowing(waveform, length);
}

void EL_UseWindowHamming(float* waveform, int length, int spfreq)
{
	CheckWindowMemory(length, spfreq, &EL_Hamming);
	EL_Windowing(waveform, length);
}