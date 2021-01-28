﻿/******************************************************************************
*  OTFFT Implementation Version 11.4xv
*
*  Copyright (c) 2015 OK Ojisan(Takuya OKAHISA)
*  Released under the MIT license
*  http://opensource.org/licenses/mit-license.php
******************************************************************************/

#include "pch.h"
#include "framework.h"

#pragma warning(disable:26451)
#pragma warning(disable:6262)

#include <cmath>
#include <cassert>
#include <stdint.h>
#include "otfft.h"
#include "otfft_misc.h"
OTFFT_MISC::okt oktinymp;
#include "otfft_avxdif4.h"
#include "otfft_avxdit4.h"
#include "otfft_avxdif8.h"
#include "otfft_avxdit8.h"
#include "otfft_avxdif16.h"
#include "otfft_avxdit16.h"
#include "otfft_sixstep.h"
#include "otfft_mixedradix.h"

namespace OTFFT { /////////////////////////////////////////////////////////////

	using FFT1 = OTFFT_AVXDIF4::FFT0;
	using FFT2 = OTFFT_AVXDIT4::FFT0;
	using FFT3 = OTFFT_AVXDIF8::FFT0;
	using FFT4 = OTFFT_AVXDIT8::FFT0;
	using FFT5 = OTFFT_AVXDIF16::FFT0;
	using FFT6 = OTFFT_AVXDIT16::FFT0;
	using FFT7 = OTFFT_SixStep::FFT0;
	using FFT8 = OTFFT_MixedRadix::FFT0;

	using namespace OTFFT_MISC;

	/******************************************************************************
	*  Complex FFT
	******************************************************************************/

	int FFT0::count = 0;

	FFT0::FFT0() noexcept : obj(0), N(0), log_N(0) { count++; }

	FFT0::FFT0(int n) : obj(0), N(n), log_N(0)
	{
		count++;
		for (; n > 1; n >>= 1) log_N++;
		if (N != (1 << log_N)) log_N = -1;
#include "otfft_gen_new.h"
		try {
#include "otfft_gen_setup.h"
		}
		catch (...) {
#include "otfft_gen_delete.h"
			throw;
		}
	}

	FFT0::~FFT0() noexcept
	{
#include "otfft_gen_delete.h"
		if (--count == 0) oktinymp.join();
	}

	void FFT0::stop() const { oktinymp.join(); }

	void FFT0::setup(int n)
	{
#include "otfft_gen_delete.h"
		for (N = n, log_N = 0; n > 1; n >>= 1) log_N++;
		if (N != (1 << log_N)) log_N = -1;
#include "otfft_gen_new.h"
		try {
#include "otfft_gen_setup.h"
		}
		catch (...) {
#include "otfft_gen_delete.h"
			throw;
		}
	}

	void FFT0::fwd(complex_vector x, complex_vector y) const noexcept
	{
#include "otfft_gen_fwd.h"
	}

	void FFT0::fwd0(complex_vector x, complex_vector y) const noexcept
	{
#include "otfft_gen_fwd0.h"
	}

	void FFT0::fwdu(complex_vector x, complex_vector y) const noexcept
	{
#include "otfft_gen_fwdu.h"
	}

	void FFT0::fwdn(complex_vector x, complex_vector y) const noexcept
	{
		fwd(x, y);
	}

	void FFT0::inv(complex_vector x, complex_vector y) const noexcept
	{
#include "otfft_gen_inv.h"
	}

	void FFT0::inv0(complex_vector x, complex_vector y) const noexcept
	{
		inv(x, y);
	}

	void FFT0::invu(complex_vector x, complex_vector y) const noexcept
	{
#include "otfft_gen_invu.h"
	}

	void FFT0::invn(complex_vector x, complex_vector y) const noexcept
	{
#include "otfft_gen_invn.h"
	}

	/******************************************************************************
	*  Real FFT
	******************************************************************************/

	RFFT::RFFT() noexcept : N(0), U(0) {}
	RFFT::RFFT(int n) { setup(n); }

	void RFFT::stop() const { oktinymp.join(); }

	void RFFT::setup(int n)
	{
		assert(n <= 1 || (n & 1) == 0);
		int log_N;
		N = n;
		for (log_N = 0; n > 1; n >>= 1) log_N++;
		fft.setup(N / 2);
		weight.setup(N + 1); U = &weight;
		const double theta0 = 2 * M_PI / N;
#if 0
		for (int p = 0; p <= N / 2; p++) {
			const double theta = p * theta0;
			const double c = cos(theta);
			const double s = -sin(theta);
			U[p] = complex_t(1 - s, c) / 2;
			U[N - p] = complex_t(1 + s, c) / 2;
		}
#else
		const int Nh = N / 2;
		const int Nq = N / 4;
		const int Ne = N / 8;
		const int Nd = N - Nq;
		if (N < 1) {}
		else if (N != (1 << log_N)) for (int p = 0; p <= Nh; p++) {
			const double theta = p * theta0;
			const double c = cos(theta);
			const double s = -sin(theta);
			U[p] = complex_t(1 - s, c) / 2;
			U[N - p] = complex_t(1 + s, c) / 2;
		}
		else if (N == 1) { U[0] = U[1] = complex_t(1, 1) / 2; }
		else if (N == 2) {
			U[0] = U[2] = complex_t(1, 1) / 2;
			U[1] = complex_t(1, -1) / 2;
		}
		else if (N == 4) {
			U[0] = complex_t(1 + 0, 1) / 2;
			U[1] = complex_t(1 + 1, 0) / 2;
			U[2] = complex_t(1 + 0, -1) / 2;
			U[3] = complex_t(1 - 1, 0) / 2;
			U[4] = complex_t(1 + 0, 1) / 2;
		}
		else if (N < OMP_THRESHOLD_W) for (int p = 0; p <= Ne; p++) {
			const double theta = p * theta0;
			const double c = cos(theta);
			const double s = -sin(theta);
			U[p] = complex_t(1 - s, c) / 2;
			U[Nq - p] = complex_t(1 + c, -s) / 2;
			U[Nq + p] = complex_t(1 + c, s) / 2;
			U[Nh - p] = complex_t(1 - s, -c) / 2;
			U[Nh + p] = complex_t(1 + s, -c) / 2;
			U[Nd - p] = complex_t(1 - c, s) / 2;
			U[Nd + p] = complex_t(1 - c, -s) / 2;
			U[N - p] = complex_t(1 + s, c) / 2;
		}
		else {
			const auto loop = [&](int) noexcept {
				const int i0 = okt::i0(Ne + 1);
				const int iN = okt::iN(Ne + 1);
				//for (int p = 0; p <= Ne; p++)
				for (int p = i0; p < iN; p++) {
					const double theta = p * theta0;
					const double c = cos(theta);
					const double s = -sin(theta);
					U[p] = complex_t(1 - s, c) / 2;
					U[Nq - p] = complex_t(1 + c, -s) / 2;
					U[Nq + p] = complex_t(1 + c, s) / 2;
					U[Nh - p] = complex_t(1 - s, -c) / 2;
					U[Nh + p] = complex_t(1 + s, -c) / 2;
					U[Nd - p] = complex_t(1 - c, s) / 2;
					U[Nd + p] = complex_t(1 - c, -s) / 2;
					U[N - p] = complex_t(1 + s, c) / 2;
				}
			};
			oktinymp.go(loop);
		}
#endif
	}

	void RFFT::fwd(const_double_vector x, complex_vector y) const noexcept
	{
		if (N < 1) return;
		else if (N == 1) { y[0] = x[0]; return; }
		const xmm rN = cmplx(1.0 / N, 1.0 / N);
		const int Nh = N / 2;
		const int Nq = N / 4;
		complex_vector z = y + Nh;
		for (int p = 0; p < Nh; p++) setpz(z[p], getpz(x + 2 * p));
		fft.fwd0(z, y);
		y[0] = (z[0].Re + z[0].Im) / N;
		z[0] = (z[0].Re - z[0].Im) / N;
		if (N < OMP_THRESHOLD) {
			for (int k = 1; k <= Nq; k++) {
				const xmm a = getpz(z[k]);
				const xmm b = cnjpz(getpz(z[Nh - k]));
				const xmm c = mulpz(getpz(U[k]), subpz(a, b));
				setpz(y[k], mulpd(rN, subpz(a, c)));
				setpz(y[Nh - k], mulpd(rN, cnjpz(addpz(b, c))));
			}
			for (int k = 1; k < Nh; k++) setpz(y[N - k], cnjpz(getpz(y[k])));
		}
		else {
			const auto loop = [&](int) noexcept {
				const int i0 = okt::i0(Nq);
				const int iN = okt::iN(Nq);
				//for (int k = 1; k <= Nq; k++)
				for (int k = i0 + 1; k < iN + 1; k++) {
					const xmm a = getpz(z[k]);
					const xmm b = cnjpz(getpz(z[Nh - k]));
					const xmm c = mulpz(getpz(U[k]), subpz(a, b));
					setpz(y[k], mulpd(rN, subpz(a, c)));
					setpz(y[Nh - k], mulpd(rN, cnjpz(addpz(b, c))));
				}
				oktinymp.wait();
				const int j0 = okt::i0(Nh - 1);
				const int jN = okt::iN(Nh - 1);
				//for (int k = 1; k < Nh; k++)
				for (int k = j0 + 1; k < jN + 1; k++)
					setpz(y[N - k], cnjpz(getpz(y[k])));
			};
			oktinymp.go(loop);
		}
	}

	void RFFT::fwd0(const_double_vector x, complex_vector y) const noexcept
	{
		if (N < 1) return;
		else if (N == 1) { y[0] = x[0]; return; }
		const int Nh = N / 2;
		const int Nq = N / 4;
		complex_vector z = y + Nh;
		for (int p = 0; p < Nh; p++) setpz(z[p], getpz(x + 2 * p));
		fft.fwd0(z, y);
		y[0] = z[0].Re + z[0].Im;
		z[0] = z[0].Re - z[0].Im;
		if (N < OMP_THRESHOLD) {
			for (int k = 1; k <= Nq; k++) {
				const xmm a = getpz(z[k]);
				const xmm b = cnjpz(getpz(z[Nh - k]));
				const xmm c = mulpz(getpz(U[k]), subpz(a, b));
				setpz(y[k], subpz(a, c));
				setpz(y[Nh - k], cnjpz(addpz(b, c)));
			}
			for (int k = 1; k < Nh; k++) setpz(y[N - k], cnjpz(getpz(y[k])));
		}
		else {
			const auto loop = [&](int) noexcept {
				const int i0 = okt::i0(Nq);
				const int iN = okt::iN(Nq);
				//for (int k = 1; k <= Nq; k++)
				for (int k = i0 + 1; k < iN + 1; k++) {
					const xmm a = getpz(z[k]);
					const xmm b = cnjpz(getpz(z[Nh - k]));
					const xmm c = mulpz(getpz(U[k]), subpz(a, b));
					setpz(y[k], subpz(a, c));
					setpz(y[Nh - k], cnjpz(addpz(b, c)));
				}
				oktinymp.wait();
				const int j0 = okt::i0(Nh - 1);
				const int jN = okt::iN(Nh - 1);
				//for (int k = 1; k < Nh; k++)
				for (int k = j0 + 1; k < jN + 1; k++)
					setpz(y[N - k], cnjpz(getpz(y[k])));
			};
			oktinymp.go(loop);
		}
	}

	void RFFT::fwdu(const_double_vector x, complex_vector y) const noexcept
	{
		if (N < 1) return;
		else if (N == 1) { y[0] = x[0]; return; }
		const double sN = sqrt(double(N));
		const xmm rsN = cmplx(1.0 / sN, 1.0 / sN);
		const int Nh = N / 2;
		const int Nq = N / 4;
		complex_vector z = y + Nh;
		for (int p = 0; p < Nh; p++) setpz(z[p], getpz(x + 2 * p));
		fft.fwd0(z, y);
		y[0] = (z[0].Re + z[0].Im) / sN;
		z[0] = (z[0].Re - z[0].Im) / sN;
		if (N < OMP_THRESHOLD) {
			for (int k = 1; k <= Nq; k++) {
				const xmm a = getpz(z[k]);
				const xmm b = cnjpz(getpz(z[Nh - k]));
				const xmm c = mulpz(getpz(U[k]), subpz(a, b));
				setpz(y[k], mulpd(rsN, subpz(a, c)));
				setpz(y[Nh - k], mulpd(rsN, cnjpz(addpz(b, c))));
			}
			for (int k = 1; k < Nh; k++) setpz(y[N - k], cnjpz(getpz(y[k])));
		}
		else {
			const auto loop = [&](int) noexcept {
				const int i0 = okt::i0(Nq);
				const int iN = okt::iN(Nq);
				//for (int k = 1; k <= Nq; k++)
				for (int k = i0 + 1; k < iN + 1; k++) {
					const xmm a = getpz(z[k]);
					const xmm b = cnjpz(getpz(z[Nh - k]));
					const xmm c = mulpz(getpz(U[k]), subpz(a, b));
					setpz(y[k], mulpd(rsN, subpz(a, c)));
					setpz(y[Nh - k], mulpd(rsN, cnjpz(addpz(b, c))));
				}
				oktinymp.wait();
				const int j0 = okt::i0(Nh - 1);
				const int jN = okt::iN(Nh - 1);
				//for (int k = 1; k < Nh; k++)
				for (int k = j0 + 1; k < jN + 1; k++)
					setpz(y[N - k], cnjpz(getpz(y[k])));
			};
			oktinymp.go(loop);
		}
	}

	void RFFT::fwdn(const_double_vector x, complex_vector y) const noexcept
	{
		fwd(x, y);
	}

	void RFFT::inv(complex_vector x, double_vector y) const noexcept
	{
		if (N < 1) return;
		else if (N == 1) { y[0] = x[0].Re; return; }
		static const xmm x2 = { 2.0, 2.0 };
		const int Nh = N / 2;
		complex_vector z = x + Nh;
		if (N < OMP_THRESHOLD) {
			for (int k = 0; k < Nh; k++) {
				const xmm a = cnjpz(getpz(x[k]));
				const xmm b = subpz(a, getpz(x[Nh - k]));
				const xmm c = mulpz(getpz(U[k]), b);
				setpz(z[k], mulpd(x2, cnjpz(subpz(a, c))));
			}
			fft.inv0(z, x);
			for (int p = 0; p < Nh; p++) setpz(y + 2 * p, getpz(z[p]));
		}
		else {
			const auto loop1 = [&](int) noexcept {
				const int i0 = okt::i0(Nh);
				const int iN = okt::iN(Nh);
				//for (int k = 0; k < Nh; k++)
				for (int k = i0; k < iN; k++) {
					const xmm a = cnjpz(getpz(x[k]));
					const xmm b = subpz(a, getpz(x[Nh - k]));
					const xmm c = mulpz(getpz(U[k]), b);
					setpz(z[k], mulpd(x2, cnjpz(subpz(a, c))));
				}
			};
			oktinymp.go(loop1);
			fft.inv0(z, x);
			const auto loop2 = [&](int) noexcept {
				const int i0 = okt::i0(Nh);
				const int iN = okt::iN(Nh);
				//for (int p = 0; p < Nh; p++)
				for (int p = i0; p < iN; p++)
					setpz(y + 2 * p, getpz(z[p]));
			};
			oktinymp.go(loop2);
		}
	}

	void RFFT::inv0(complex_vector x, double_vector y) const noexcept
	{
		inv(x, y);
	}

	void RFFT::invu(complex_vector x, double_vector y) const noexcept
	{
		if (N < 1) return;
		else if (N == 1) { y[0] = x[0].Re; return; }
		const double s2dsN = 2.0 / sqrt(N);
		const xmm x2dsN = cmplx(s2dsN, s2dsN);
		const int Nh = N / 2;
		complex_vector z = x + Nh;
		if (N < OMP_THRESHOLD) {
			for (int k = 0; k < Nh; k++) {
				const xmm a = cnjpz(getpz(x[k]));
				const xmm b = subpz(a, getpz(x[Nh - k]));
				const xmm c = mulpz(getpz(U[k]), b);
				setpz(z[k], mulpd(x2dsN, cnjpz(subpz(a, c))));
			}
			fft.inv0(z, x);
			for (int p = 0; p < Nh; p++) setpz(y + 2 * p, getpz(z[p]));
		}
		else {
			const auto loop1 = [&](int) noexcept {
				const int i0 = okt::i0(Nh);
				const int iN = okt::iN(Nh);
				//for (int k = 0; k < Nh; k++)
				for (int k = i0; k < iN; k++) {
					const xmm a = cnjpz(getpz(x[k]));
					const xmm b = subpz(a, getpz(x[Nh - k]));
					const xmm c = mulpz(getpz(U[k]), b);
					setpz(z[k], mulpd(x2dsN, cnjpz(subpz(a, c))));
				}
			};
			oktinymp.go(loop1);
			fft.inv0(z, x);
			const auto loop2 = [&](int) noexcept {
				const int i0 = okt::i0(Nh);
				const int iN = okt::iN(Nh);
				//for (int p = 0; p < Nh; p++)
				for (int p = i0; p < iN; p++)
					setpz(y + 2 * p, getpz(z[p]));
			};
			oktinymp.go(loop2);
		}
	}

	void RFFT::invn(complex_vector x, double_vector y) const noexcept
	{
		if (N < 1) return;
		else if (N == 1) { y[0] = x[0].Re; return; }
		const int Nh = N / 2;
		complex_vector z = x + Nh;
		if (N < OMP_THRESHOLD) {
			for (int k = 0; k < Nh; k++) {
				const xmm a = cnjpz(getpz(x[k]));
				const xmm b = subpz(a, getpz(x[Nh - k]));
				const xmm c = mulpz(getpz(U[k]), b);
				setpz(z[k], cnjpz(subpz(a, c)));
			}
			fft.invn(z, x);
			for (int p = 0; p < Nh; p++) setpz(y + 2 * p, getpz(z[p]));
		}
		else {
			const auto loop1 = [&](int) noexcept {
				const int i0 = okt::i0(Nh);
				const int iN = okt::iN(Nh);
				//for (int k = 0; k < Nh; k++)
				for (int k = i0; k < iN; k++) {
					const xmm a = cnjpz(getpz(x[k]));
					const xmm b = subpz(a, getpz(x[Nh - k]));
					const xmm c = mulpz(getpz(U[k]), b);
					setpz(z[k], cnjpz(subpz(a, c)));
				}
			};
			oktinymp.go(loop1);
			fft.invn(z, x);
			const auto loop2 = [&](int) noexcept {
				const int i0 = okt::i0(Nh);
				const int iN = okt::iN(Nh);
				//for (int p = 0; p < Nh; p++)
				for (int p = i0; p < iN; p++)
					setpz(y + 2 * p, getpz(z[p]));
			};
			oktinymp.go(loop2);
		}
	}

	/******************************************************************************
	*  DCT-II
	******************************************************************************/

	DCT0::DCT0() noexcept : N(0), V(0) {}
	DCT0::DCT0(int n) { setup(n); }

	void DCT0::stop() const { oktinymp.join(); }

	void DCT0::setup(int n)
	{
		assert(n <= 1 || (n & 1) == 0);
		N = n;
		rfft.setup(N);
		weight.setup(N + 1); V = &weight;
		const double theta0 = M_PI / (2 * N);
		const int Nh = N / 2;
		if (N < 2) {}
		else if (N < OMP_THRESHOLD_W) for (int p = 0; p <= Nh; p++) {
			const double theta = p * theta0;
			const double c = cos(theta);
			const double s = sin(theta);
			V[p] = complex_t(c, s);
			V[N - p] = complex_t(s, c);
		}
		else {
			const auto loop = [&](int) noexcept {
				const int i0 = okt::i0(Nh + 1);
				const int iN = okt::iN(Nh + 1);
				//for (int p = 0; p <= Nh; p++)
				for (int p = i0; p < iN; p++) {
					const double theta = p * theta0;
					const double c = cos(theta);
					const double s = sin(theta);
					V[p] = complex_t(c, s);
					V[N - p] = complex_t(s, c);
				}
			};
			oktinymp.go(loop);
		}
	}

	void DCT0::fwd(double_vector x, double_vector y, complex_vector z) const noexcept
	{
		if (N < 2) return;
		const int Nh = N / 2;
		if (N < OMP_THRESHOLD) {
			for (int p = 0; p < Nh; p++) {
				y[p] = x[2 * p + 0];
				y[N - p - 1] = x[2 * p + 1];
			}
		}
		else {
			const auto loop = [&](int) noexcept {
				const int i0 = okt::i0(Nh);
				const int iN = okt::iN(Nh);
				//for (int p = 0; p < Nh; p++)
				for (int p = i0; p < iN; p++) {
					y[p] = x[2 * p + 0];
					y[N - p - 1] = x[2 * p + 1];
				}
			};
			oktinymp.go(loop);
		}
		rfft.fwd(y, z);
		if (N < OMP_THRESHOLD) {
			//for (int k = 0; k < N; k++)
			//    x[k] = V[k].Re*z[k].Re + V[k].Im*z[k].Im;
			for (int k = 0; k < N; k += 2) {
				const xmm a = mulpd(getpz(V[k + 0]), getpz(z[k + 0]));
				const xmm b = mulpd(getpz(V[k + 1]), getpz(z[k + 1]));
				setpz(x + k, haddpz(a, b));
			}
		}
		else {
			const auto loop = [&](int) noexcept {
				const int i0 = okt::i0(N / 2);
				const int iN = okt::iN(N / 2);
				//for (int k = 0; k < N; k += 2)
				for (int i = i0; i < iN; i++) {
					const int k = 2 * i;
					const xmm a = mulpd(getpz(V[k + 0]), getpz(z[k + 0]));
					const xmm b = mulpd(getpz(V[k + 1]), getpz(z[k + 1]));
					setpz(x + k, haddpz(a, b));
				}
			};
			oktinymp.go(loop);
		}
	}

	void DCT0::fwd0(double_vector x, double_vector y, complex_vector z) const noexcept
	{
		if (N < 2) return;
		const int Nh = N / 2;
		if (N < OMP_THRESHOLD) {
			for (int p = 0; p < Nh; p++) {
				y[p] = x[2 * p + 0];
				y[N - p - 1] = x[2 * p + 1];
			}
		}
		else {
			const auto loop = [&](int) noexcept {
				const int i0 = okt::i0(Nh);
				const int iN = okt::iN(Nh);
				//for (int p = 0; p < Nh; p++)
				for (int p = i0; p < iN; p++) {
					y[p] = x[2 * p + 0];
					y[N - p - 1] = x[2 * p + 1];
				}
			};
			oktinymp.go(loop);
		}
		rfft.fwd0(y, z);
		if (N < OMP_THRESHOLD) {
			for (int k = 0; k < N; k += 2) {
				const xmm a = mulpd(getpz(V[k + 0]), getpz(z[k + 0]));
				const xmm b = mulpd(getpz(V[k + 1]), getpz(z[k + 1]));
				setpz(x + k, haddpz(a, b));
			}
		}
		else {
			const auto loop = [&](int) noexcept {
				const int i0 = okt::i0(N / 2);
				const int iN = okt::iN(N / 2);
				//for (int k = 0; k < N; k += 2)
				for (int i = i0; i < iN; i++) {
					const int k = 2 * i;
					const xmm a = mulpd(getpz(V[k + 0]), getpz(z[k + 0]));
					const xmm b = mulpd(getpz(V[k + 1]), getpz(z[k + 1]));
					setpz(x + k, haddpz(a, b));
				}
			};
			oktinymp.go(loop);
		}
	}

	void DCT0::fwdn(double_vector x, double_vector y, complex_vector z) const noexcept
	{
		fwd(x, y, z);
	}

	void DCT0::inv(double_vector x, double_vector y, complex_vector z) const noexcept
	{
		if (N < 2) return;
		const int Nh = N / 2;
		z[0] = x[0];
		if (N < OMP_THRESHOLD) {
			for (int k = 1; k < N; k++) z[k] = V[k] * complex_t(x[k], -x[N - k]);
		}
		else {
			const auto loop = [&](int) noexcept {
				const int i0 = okt::i0(N - 1);
				const int iN = okt::iN(N - 1);
				//for (int k = 1; k < N; k++)
				for (int k = i0 + 1; k < iN + 1; k++)
					z[k] = V[k] * complex_t(x[k], -x[N - k]);
			};
			oktinymp.go(loop);
		}
		rfft.inv(z, y);
		if (N < OMP_THRESHOLD) {
			for (int p = 0; p < Nh; p++) {
				x[2 * p + 0] = y[p];
				x[2 * p + 1] = y[N - p - 1];
			}
		}
		else {
			const auto loop = [&](int) noexcept {
				const int i0 = okt::i0(Nh);
				const int iN = okt::iN(Nh);
				//for (int p = 0; p < Nh; p++)
				for (int p = i0; p < iN; p++) {
					x[2 * p + 0] = y[p];
					x[2 * p + 1] = y[N - p - 1];
				}
			};
			oktinymp.go(loop);
		}
	}

	void DCT0::inv0(double_vector x, double_vector y, complex_vector z) const noexcept
	{
		inv(x, y, z);
	}

	void DCT0::invn(double_vector x, double_vector y, complex_vector z) const noexcept
	{
		if (N < 2) return;
		const int Nh = N / 2;
		z[0] = x[0];
		if (N < OMP_THRESHOLD) {
			for (int k = 1; k < N; k++) z[k] = V[k] * complex_t(x[k], -x[N - k]);
		}
		else {
			const auto loop = [&](int) noexcept {
				const int i0 = okt::i0(N - 1);
				const int iN = okt::iN(N - 1);
				//for (int k = 1; k < N; k++)
				for (int k = i0 + 1; k < iN + 1; k++)
					z[k] = V[k] * complex_t(x[k], -x[N - k]);
			};
			oktinymp.go(loop);
		}
		rfft.invn(z, y);
		if (N < OMP_THRESHOLD) {
			for (int p = 0; p < Nh; p++) {
				x[2 * p + 0] = y[p];
				x[2 * p + 1] = y[N - p - 1];
			}
		}
		else {
			const auto loop = [&](int) noexcept {
				const int i0 = okt::i0(Nh);
				const int iN = okt::iN(Nh);
				//for (int p = 0; p < Nh; p++)
				for (int p = i0; p < iN; p++) {
					x[2 * p + 0] = y[p];
					x[2 * p + 1] = y[N - p - 1];
				}
			};
			oktinymp.go(loop);
		}
	}

	/******************************************************************************
	*  Bluestein's FFT
	******************************************************************************/

	Bluestein::Bluestein() noexcept : N(0), L(0), a(0), b(0), W(0) {}
	Bluestein::Bluestein(int n) { setup(n); }

	void Bluestein::stop() const { oktinymp.join(); }

	void Bluestein::setup(int n)
	{
		if (n < 1) return;
		N = n;
		const int N2 = 2 * N;
		for (L = 1; L < N2 - 1; L *= 2);
		fft.setup(L);
		work1.setup(L); a = &work1;
		work2.setup(L); b = &work2;
		weight.setup(N2 + 1); W = &weight;
		const double theta0 = M_PI / N;
		W[0] = W[N2] = 1; W[N] = -1;
		if (N < OMP_THRESHOLD_W) for (int p = 1; p < N; p++) {
			const double theta = p * theta0;
			const double c = cos(theta);
			const double s = -sin(theta);
			W[p] = complex_t(c, s);
			W[N2 - p] = complex_t(c, -s);
		}
		else {
			const auto loop = [&](int) noexcept {
				const int i0 = okt::i0(N - 1);
				const int iN = okt::iN(N - 1);
				//for (int p = 1; p < N; p++)
				for (int p = i0 + 1; p < iN + 1; p++) {
					const double theta = p * theta0;
					const double c = cos(theta);
					const double s = -sin(theta);
					W[p] = complex_t(c, s);
					W[N2 - p] = complex_t(c, -s);
				}
			};
			oktinymp.go(loop);
		}
	}

	void Bluestein::fwd(complex_vector x) const noexcept
	{
		if (N < 2) return;
		const xmm rN = cmplx(1.0 / N, 1.0 / N);
		const int N2 = 2 * N;
		a[0] = x[0]; b[0] = x[0] = 1;
		if (N < OMP_THRESHOLD) {
			for (int p = 1; p < L; p++) a[p] = b[p] = 0;
			for (int p = 1; p < N; p++) {
				const int64_t q = p;
				const int pp = static_cast<int>(q * q % N2);
				a[p] = x[p] * W[pp];
				b[p] = x[p] = W[N2 - pp];
				b[L - p] = b[p];
			}
		}
		else {
			const auto loop = [&](int) noexcept {
				const int i0 = okt::i0(L - 1);
				const int iN = okt::iN(L - 1);
				//for (int p = 1; p < L; p++)
				for (int p = i0 + 1; p < iN + 1; p++)
					a[p] = b[p] = 0;
				oktinymp.wait();
				const int j0 = okt::i0(N - 1);
				const int jN = okt::iN(N - 1);
				//for (int p = 1; p < N; p++)
				for (int p = j0 + 1; p < jN + 1; p++) {
					const int64_t q = p;
					const int pp = static_cast<int>(q * q % N2);
					a[p] = x[p] * W[pp];
					b[p] = x[p] = W[N2 - pp];
					b[L - p] = b[p];
				}
			};
			oktinymp.go(loop);
		}
		fft.fwd0(a); fft.fwd0(b);
		if (N < OMP_THRESHOLD) {
			for (int k = 0; k < L; k++)
				setpz(a[k], mulpz(getpz(a[k]), getpz(b[k])));
			fft.invn(a);
			for (int p = 0; p < N; p++)
				setpz(x[p], mulpd(rN, mulpz(getpz(a[p]), cnjpz(getpz(x[p])))));
		}
		else {
			const auto loop1 = [&](int) noexcept {
				const int i0 = okt::i0(L);
				const int iN = okt::iN(L);
				//for (int k = 0; k < L; k++)
				for (int k = i0; k < iN; k++)
					setpz(a[k], mulpz(getpz(a[k]), getpz(b[k])));
			};
			oktinymp.go(loop1);
			fft.invn(a);
			const auto loop2 = [&](int) noexcept {
				const int i0 = okt::i0(N);
				const int iN = okt::iN(N);
				//for (int p = 0; p < N; p++)
				for (int p = i0; p < iN; p++)
					setpz(x[p], mulpd(rN, mulpz(getpz(a[p]), cnjpz(getpz(x[p])))));
			};
			oktinymp.go(loop2);
		}
	}

	void Bluestein::fwd0(complex_vector x) const noexcept
	{
		if (N < 2) return;
		const int N2 = 2 * N;
		a[0] = x[0]; b[0] = x[0] = 1;
		if (N < OMP_THRESHOLD) {
			for (int p = 1; p < L; p++) a[p] = b[p] = 0;
			for (int p = 1; p < N; p++) {
				const int64_t q = p;
				const int pp = static_cast<int>(q * q % N2);
				a[p] = x[p] * W[pp];
				b[p] = x[p] = W[N2 - pp];
				b[L - p] = b[p];
			}
		}
		else {
			const auto loop = [&](int) noexcept {
				const int i0 = okt::i0(L - 1);
				const int iN = okt::iN(L - 1);
				//for (int p = 1; p < L; p++)
				for (int p = i0 + 1; p < iN + 1; p++)
					a[p] = b[p] = 0;
				oktinymp.wait();
				const int j0 = okt::i0(N - 1);
				const int jN = okt::iN(N - 1);
				//for (int p = 1; p < N; p++)
				for (int p = j0 + 1; p < jN + 1; p++) {
					const int64_t q = p;
					const int pp = static_cast<int>(q * q % N2);
					a[p] = x[p] * W[pp];
					b[p] = x[p] = W[N2 - pp];
					b[L - p] = b[p];
				}
			};
			oktinymp.go(loop);
		}
		fft.fwd0(a); fft.fwd0(b);
		if (N < OMP_THRESHOLD) {
			for (int k = 0; k < L; k++)
				setpz(a[k], mulpz(getpz(a[k]), getpz(b[k])));
			fft.invn(a);
			for (int p = 0; p < N; p++)
				setpz(x[p], mulpz(getpz(a[p]), cnjpz(getpz(x[p]))));
		}
		else {
			const auto loop1 = [&](int) noexcept {
				const int i0 = okt::i0(L);
				const int iN = okt::iN(L);
				//for (int k = 0; k < L; k++)
				for (int k = i0; k < iN; k++)
					setpz(a[k], mulpz(getpz(a[k]), getpz(b[k])));
			};
			oktinymp.go(loop1);
			fft.invn(a);
			const auto loop2 = [&](int) noexcept {
				const int i0 = okt::i0(N);
				const int iN = okt::iN(N);
				//for (int p = 0; p < N; p++)
				for (int p = i0; p < iN; p++)
					setpz(x[p], mulpz(getpz(a[p]), cnjpz(getpz(x[p]))));
			};
			oktinymp.go(loop2);
		}
	}

	void Bluestein::fwdu(complex_vector x) const noexcept
	{
		if (N < 2) return;
		const double ssrN = sqrt(1.0 / N);
		const xmm srN = cmplx(ssrN, ssrN);
		const int N2 = 2 * N;
		a[0] = x[0]; b[0] = x[0] = 1;
		if (N < OMP_THRESHOLD) {
			for (int p = 1; p < L; p++) a[p] = b[p] = 0;
			for (int p = 1; p < N; p++) {
				const int64_t q = p;
				const int pp = static_cast<int>(q * q % N2);
				a[p] = x[p] * W[pp];
				b[p] = x[p] = W[N2 - pp];
				b[L - p] = b[p];
			}
		}
		else {
			const auto loop = [&](int) noexcept {
				const int i0 = okt::i0(L - 1);
				const int iN = okt::iN(L - 1);
				//for (int p = 1; p < L; p++)
				for (int p = i0 + 1; p < iN + 1; p++)
					a[p] = b[p] = 0;
				oktinymp.wait();
				const int j0 = okt::i0(N - 1);
				const int jN = okt::iN(N - 1);
				//for (int p = 1; p < N; p++)
				for (int p = j0 + 1; p < jN + 1; p++) {
					const int64_t q = p;
					const int pp = static_cast<int>(q * q % N2);
					a[p] = x[p] * W[pp];
					b[p] = x[p] = W[N2 - pp];
					b[L - p] = b[p];
				}
			};
			oktinymp.go(loop);
		}
		fft.fwd0(a); fft.fwd0(b);
		if (N < OMP_THRESHOLD) {
			for (int k = 0; k < L; k++)
				setpz(a[k], mulpz(getpz(a[k]), getpz(b[k])));
			fft.invn(a);
			for (int p = 0; p < N; p++)
				setpz(x[p], mulpd(srN, mulpz(getpz(a[p]), cnjpz(getpz(x[p])))));
		}
		else {
			const auto loop1 = [&](int) noexcept {
				const int i0 = okt::i0(L);
				const int iN = okt::iN(L);
				//for (int k = 0; k < L; k++)
				for (int k = i0; k < iN; k++)
					setpz(a[k], mulpz(getpz(a[k]), getpz(b[k])));
			};
			oktinymp.go(loop1);
			fft.invn(a);
			const auto loop2 = [&](int) noexcept {
				const int i0 = okt::i0(N);
				const int iN = okt::iN(N);
				//for (int p = 0; p < N; p++)
				for (int p = i0; p < iN; p++)
					setpz(x[p], mulpd(srN, mulpz(getpz(a[p]), cnjpz(getpz(x[p])))));
			};
			oktinymp.go(loop2);
		}
	}

	void Bluestein::fwdn(complex_vector x) const noexcept { fwd(x); }

	void Bluestein::inv(complex_vector x) const noexcept
	{
		if (N < 2) return;
		const int N2 = 2 * N;
		a[0] = x[0]; b[0] = x[0] = 1;
		if (N < OMP_THRESHOLD) {
			for (int p = 1; p < L; p++) a[p] = b[p] = 0;
			for (int p = 1; p < N; p++) {
				const int64_t q = p;
				const int pp = static_cast<int>(q * q % N2);
				a[p] = x[p] * W[N2 - pp];
				b[p] = x[p] = W[pp];
				b[L - p] = b[p];
			}
		}
		else {
			const auto loop = [&](int) noexcept {
				const int i0 = okt::i0(L - 1);
				const int iN = okt::iN(L - 1);
				//for (int p = 1; p < L; p++)
				for (int p = i0 + 1; p < iN + 1; p++)
					a[p] = b[p] = 0;
				oktinymp.wait();
				const int j0 = okt::i0(N - 1);
				const int jN = okt::iN(N - 1);
				//for (int p = 1; p < N; p++)
				for (int p = j0 + 1; p < jN + 1; p++) {
					const int64_t q = p;
					const int pp = static_cast<int>(q * q % N2);
					a[p] = x[p] * W[N2 - pp];
					b[p] = x[p] = W[pp];
					b[L - p] = b[p];
				}
			};
			oktinymp.go(loop);
		}
		fft.fwd0(a); fft.fwd0(b);
		if (N < OMP_THRESHOLD) {
			for (int k = 0; k < L; k++)
				setpz(a[k], mulpz(getpz(a[k]), getpz(b[k])));
			fft.invn(a);
			for (int p = 0; p < N; p++)
				setpz(x[p], mulpz(getpz(a[p]), cnjpz(getpz(x[p]))));
		}
		else {
			const auto loop1 = [&](int) noexcept {
				const int i0 = okt::i0(L);
				const int iN = okt::iN(L);
				//for (int k = 0; k < L; k++)
				for (int k = i0; k < iN; k++)
					setpz(a[k], mulpz(getpz(a[k]), getpz(b[k])));
			};
			oktinymp.go(loop1);
			fft.invn(a);
			const auto loop2 = [&](int) noexcept {
				const int i0 = okt::i0(N);
				const int iN = okt::iN(N);
				//for (int p = 0; p < N; p++)
				for (int p = i0; p < iN; p++)
					setpz(x[p], mulpz(getpz(a[p]), cnjpz(getpz(x[p]))));
			};
			oktinymp.go(loop2);
		}
	}

	void Bluestein::inv0(complex_vector x) const noexcept { inv(x); }

	void Bluestein::invu(complex_vector x) const noexcept
	{
		if (N < 2) return;
		const double ssrN = sqrt(1.0 / N);
		const xmm srN = cmplx(ssrN, ssrN);
		const int N2 = 2 * N;
		a[0] = x[0]; b[0] = x[0] = 1;
		if (N < OMP_THRESHOLD) {
			for (int p = 1; p < L; p++) a[p] = b[p] = 0;
			for (int p = 1; p < N; p++) {
				const int64_t q = p;
				const int pp = static_cast<int>(q * q % N2);
				a[p] = x[p] * W[N2 - pp];
				b[p] = x[p] = W[pp];
				b[L - p] = b[p];
			}
		}
		else {
			const auto loop = [&](int) noexcept {
				const int i0 = okt::i0(L - 1);
				const int iN = okt::iN(L - 1);
				//for (int p = 1; p < L; p++)
				for (int p = i0 + 1; p < iN + 1; p++)
					a[p] = b[p] = 0;
				oktinymp.wait();
				const int j0 = okt::i0(N - 1);
				const int jN = okt::iN(N - 1);
				//for (int p = 1; p < N; p++)
				for (int p = j0 + 1; p < jN + 1; p++) {
					const int64_t q = p;
					const int pp = static_cast<int>(q * q % N2);
					a[p] = x[p] * W[N2 - pp];
					b[p] = x[p] = W[pp];
					b[L - p] = b[p];
				}
			};
			oktinymp.go(loop);
		}
		fft.fwd0(a); fft.fwd0(b);
		if (N < OMP_THRESHOLD) {
			for (int k = 0; k < L; k++)
				setpz(a[k], mulpz(getpz(a[k]), getpz(b[k])));
			fft.invn(a);
			for (int p = 0; p < N; p++)
				setpz(x[p], mulpd(srN, mulpz(getpz(a[p]), cnjpz(getpz(x[p])))));
		}
		else {
			const auto loop1 = [&](int) noexcept {
				const int i0 = okt::i0(L);
				const int iN = okt::iN(L);
				//for (int k = 0; k < L; k++)
				for (int k = i0; k < iN; k++)
					setpz(a[k], mulpz(getpz(a[k]), getpz(b[k])));
			};
			oktinymp.go(loop1);
			fft.invn(a);
			const auto loop2 = [&](int) noexcept {
				const int i0 = okt::i0(N);
				const int iN = okt::iN(N);
				//for (int p = 0; p < N; p++)
				for (int p = i0; p < iN; p++)
					setpz(x[p], mulpd(srN, mulpz(getpz(a[p]), cnjpz(getpz(x[p])))));
			};
			oktinymp.go(loop2);
		}
	}

	void Bluestein::invn(complex_vector x) const noexcept
	{
		if (N < 2) return;
		const xmm rN = cmplx(1.0 / N, 1.0 / N);
		const int N2 = 2 * N;
		a[0] = x[0]; b[0] = x[0] = 1;
		if (N < OMP_THRESHOLD) {
			for (int p = 1; p < L; p++) a[p] = b[p] = 0;
			for (int p = 1; p < N; p++) {
				const int64_t q = p;
				const int pp = static_cast<int>(q * q % N2);
				a[p] = x[p] * W[N2 - pp];
				b[p] = x[p] = W[pp];
				b[L - p] = b[p];
			}
		}
		else {
			const auto loop = [&](int) noexcept {
				const int i0 = okt::i0(L - 1);
				const int iN = okt::iN(L - 1);
				//for (int p = 1; p < L; p++)
				for (int p = i0 + 1; p < iN + 1; p++)
					a[p] = b[p] = 0;
				oktinymp.wait();
				const int j0 = okt::i0(N - 1);
				const int jN = okt::iN(N - 1);
				//for (int p = 1; p < N; p++)
				for (int p = j0 + 1; p < jN + 1; p++) {
					const int64_t q = p;
					const int pp = static_cast<int>(q * q % N2);
					a[p] = x[p] * W[N2 - pp];
					b[p] = x[p] = W[pp];
					b[L - p] = b[p];
				}
			};
			oktinymp.go(loop);
		}
		fft.fwd0(a); fft.fwd0(b);
		if (N < OMP_THRESHOLD) {
			for (int k = 0; k < L; k++)
				setpz(a[k], mulpz(getpz(a[k]), getpz(b[k])));
			fft.invn(a);
			for (int p = 0; p < N; p++)
				setpz(x[p], mulpd(rN, mulpz(getpz(a[p]), cnjpz(getpz(x[p])))));
		}
		else {
			const auto loop1 = [&](int) noexcept {
				const int i0 = okt::i0(L);
				const int iN = okt::iN(L);
				//for (int k = 0; k < L; k++)
				for (int k = i0; k < iN; k++)
					setpz(a[k], mulpz(getpz(a[k]), getpz(b[k])));
			};
			oktinymp.go(loop1);
			fft.invn(a);
			const auto loop2 = [&](int) noexcept {
				const int i0 = okt::i0(N);
				const int iN = okt::iN(N);
				//for (int p = 0; p < N; p++)
				for (int p = i0; p < iN; p++)
					setpz(x[p], mulpd(rN, mulpz(getpz(a[p]), cnjpz(getpz(x[p])))));
			};
			oktinymp.go(loop2);
		}
	}

} /////////////////////////////////////////////////////////////////////////////

#ifndef _MSC_VER
#include "otfft_c.cpp"
#endif

///////////////////////////////////////////////////////////////////////////////
