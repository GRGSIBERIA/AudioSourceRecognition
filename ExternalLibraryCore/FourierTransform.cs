using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.InteropServices;

namespace ExternalLibraryCore
{
    public class FourierTransform
    {
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        struct complex_t
        {
            public float re;
            public float im;

            public complex_t(float re = 0, float im = 0) 
            { 
                this.re = re; 
                this.im = im; 
            }
        }

        const int avx = 8;
        int length;
        int spfreq;

        Dictionary<int, float> theta0_table;

        complex_t[] input;      // 入力用の配列
        complex_t[] output;     // 出力用の配列
        float[] worker = new float[8];

        float[] spectrums;      // スペクトル用の配列
        AbstractWindow window;  // 窓関数

        /// <summary>
        /// フーリエ変換
        /// </summary>
        /// <param name="window">窓関数</param>
        /// <param name="samplingFreq">サンプリング周波数</param>
        public FourierTransform(AbstractWindow window, int samplingFreq)
        {
            // windowの窓幅とサンプル数は合致させなければならない

            this.window = window;
            this.length = window.Length;
            this.spfreq = samplingFreq;

            // 実部は窓掛けしたものを代入する
            this.input = new complex_t[this.length];
            this.output = new complex_t[this.length];

            // スペクトルは半分だけど全部出しておく
            this.spectrums = new float[this.length];

            // thetaテーブル
            this.theta0_table = new Dictionary<int, float>();
            for (int n = this.length; n != 2; n = n >> 1)
            {
                theta0_table.Add(n, 2f * MathF.PI / n);
            }
        }

        private unsafe Vector256<float> LoadSinCosTable(int p, float theta0)
        {
            Vector256<float> wp;
            worker[0] = MathF.Cos(p * theta0);
            worker[1] = -MathF.Sin(p * theta0);
            for (int i = 1; i < 8; ++i)
            {
                worker[i] = (i & 1) == 0 ? worker[0] : worker[1];
            }   // シャッフルするよりコピーしたほうが速いかもしれない
            fixed (float* workerp = worker)
                wp = Avx.LoadVector256(workerp);

            return wp;
        }

        private unsafe void fft0(int n, int s, bool eo, complex_t* x, complex_t* y)
        {
            int m = n >> 1;
            float theta0 = theta0_table[n];

            if (n == 2)
            {   // ストライドが2倍ずつ増えているのに、4以下になることは考えない
                complex_t* z = eo ? y : x;
                for (int q = 0; q < s; q += 4)
                {
                    // 原文はq + 2 * sとなっているが、
                    // 2 * s項はcomplex_tポインタ上では半分になる
                    // AVX化前のコードを辿るとその通りになっている

                    Vector256<float> a, b;
                    float* ap = &x[q + 0].re;
                    float* bp = &x[q + s].re;

                    a = Avx.LoadVector256(ap);
                    b = Avx.LoadVector256(bp);

                    float* azp = &z[q + 0].re;
                    float* bzp = &z[q + s].re;

                    Avx.Store(azp, Avx.Add(a, b));
                    Avx.Store(bzp, Avx.Subtract(a, b));
                    // ポインタは4と8の倍数になるものとする
                }
            }
            else if (n >= 4)
            {
                if (s >= 4)
                {   // ストライドが十分に広いなら計算できる
                    for (int p = 0; p < m; ++p)
                    {
                        // サインコサインのテーブルを作る
                        Vector256<float> wp = LoadSinCosTable(p, theta0);
                        Vector256<float> a, b, yy;
                        for (int q = 0; q < s; q += 4)
                        {
                            float* ap = &x[q + s * (p + 0)].re;
                            float* bp = &x[q + s * (p + m)].re;

                            float* y1p = &y[q + s * (2 * p + 0)].re;
                            float* y2p = &y[q + s * (2 * p + 1)].re;

                            a = Avx.LoadVector256(ap);
                            b = Avx.LoadVector256(bp);
                            yy = Avx.Add(a, b);
                            Avx.Store(y1p, yy); // 1個目の演算 (a+b)

                            yy = Avx.Subtract(a, b);
                            Vector256<float> yx;
                            a = Avx.UnpackLow(yy, yy);
                            b = Avx.UnpackHigh(yy, yy);
                            yx = Avx.Shuffle(wp, wp, 0x55);
                            yx = Avx.AddSubtract(Avx.Multiply(a, yy), Avx.Multiply(b, yx));
                            Avx.Store(y2p, yx); // 2個目の演算 (a-b)*w
                        }
                    }
                }
                else if (s == 2)
                {   // ここもストライドが短い
                    float w0, w1;
                    for (int p = 0; p < m; ++p)
                    {
                        // LoadSinCosTable(p, theta0);
                        w0 = MathF.Cos(p * theta0);
                        w1 = -MathF.Sin(p * theta0);

                        for (int q = 0; q < s; ++q)
                        {   // Nの数が十分に大きいとこの中の計算が長くなる
                            float* a = &x[q + s * (p + 0)].re;
                            float* b = &x[q + s * (p + m)].re;
                            float* y1 = &y[q + s * (2 * p + 0)].re;
                            float* y2 = &y[q + s * (2 * p + 1)].re;

                            y1[0] = a[0] + b[0];
                            y1[1] = a[1] + b[1];

                            y2[0] = a[0] - b[0];
                            y2[1] = a[1] - b[1];
                            y2[0] = y2[0] * w0 - y2[1] * w1;
                            y2[1] = y2[0] * w1 + y2[1] * w0;
                        }
                    }
                }
                else if (s == 1)
                {   // ストライドが狭すぎるパターン
                    float w0, w1;
                    
                    for (int p = 0; p < m; ++p)
                    {
                        // LoadSinCosTable(p, theta0);
                        w0 = MathF.Cos(p * theta0);
                        w1 = -MathF.Sin(p * theta0);
                        float* a = &x[p + 0].re;
                        float* b = &x[p + m].re;
                        float* y1 = &y[2 * p + 0].re;
                        float* y2 = &y[2 * p + 1].re;

                        y1[0] = a[0] + b[0];
                        y1[1] = a[1] + b[1];

                        y2[0] = a[0] - b[0];
                        y2[1] = a[1] - b[1];
                        y2[0] = y2[0] * w0 - y2[1] * w1;
                        y2[1] = y2[0] * w1 + y2[1] * w0;
                        
                    }
                }
                fft0(m, s << 1, !eo, y, x);
            }
        }
        
        unsafe void InitializeInputComplex(float[] windowed)
        {
            for (int i = 0; i < this.length; i += 4)
            {
                Vector128<float> a, z;
                
                fixed (float* wp = &windowed[i])
                {
                    Sse.Prefetch0(wp + 4);
                    a = Sse.LoadVector128(wp);
                    z = Sse.Xor(a, a);  // zeroベクトル
                }

                // a -- 3 2 1 0 load
                // z -- X X X X xor(a,a)
                // z -- X 1 X 0 unpacklo(a,z)
                // z -- X X X X xor(z,z)
                // z -- X 3 X 2 unpackhi(a,z)
                
                fixed (complex_t* cp = &input[i])
                {
                    z = Sse.UnpackLow(a, z);
                    Sse.Store(&cp[0].re, z);
                    z = Sse.Xor(z, z);
                    z = Sse.UnpackHigh(a, z);
                    Sse.Store(&cp[1].re, z);
                }
            }
        }

        /// <summary>
        /// Nの逆数を全体にかける関数
        /// </summary>
        /// <param name="output">出力</param>
        /// <param name="N">系列数</param>
        unsafe void InvN(complex_t* output, int N)
        {
            Vector256<float> wk, nk;
            float invn = 1f / (float)N;
            float[] invna = new float[8]
            {
                invn, invn, invn, invn,
                invn, invn, invn, invn
            };
            for (int i = 0; i < N; i += 4)
            {
                wk = Avx.LoadVector256(&output[i].re);
                fixed (float* inv = invna)
                    nk = Avx.LoadVector256(inv);
                wk = Avx.Multiply(wk, nk);
                Avx.Store(&output[i].re, wk);
            }
        }

        unsafe void ComputePowerSpectrum(complex_t* output, float* fourier, int N)
        {
            for (int i = 0; i < N; i += 4)
            {
                float* cp = &output[i].re;

                // a --- r1 i1 r2 i2 r3 i3 r4 i4 movps
                // b --- r1 r1 r2 r2 r3 r3 r4 r4 unpacklo(a,a)
                // a --- i1 i1 i2 i2 r3 r3 r4 r4 unpackhi(a,a)
                // i^2 = mul(a,a)
                // r^2 = mul(b,b)
                // a --- i^2 + r^2 add(a,b)
                // sqrt(i^2+r^2) = sqrt(a)
                // a --- p1 p1 p2 p2 p3 p3 p4 p4

                Vector256<float> a, b;

                a = Avx.LoadVector256(cp);
                b = Avx.UnpackLow(a, a);
                a = Avx.UnpackHigh(a, a);
                a = Avx.Sqrt(
                    Avx.Add(
                        Avx.Multiply(a, a), 
                        Avx.Multiply(b, b)));

                for (int j = 0; j < 4; ++j)
                    fourier[i * 4 + j * 2] = a.GetElement(j);
            }
        }

        /// <summary>
        /// 高速フーリエ変換
        /// </summary>
        /// <param name="waveform">波形</param>
        /// <returns>パワースペクトル</returns>
        public float[] FFT(float[] waveform)
        {
            float[] windowed = window.UseWindowing(waveform, this.spfreq);

            InitializeInputComplex(windowed);
            unsafe
            {
                fixed (complex_t* outp = output)
                {
                    fixed (complex_t* inp = input)
                    {
                        fft0(windowed.Length, 1, false, inp, outp);
                    }

                    InvN(outp, windowed.Length);    // Nの逆数で正規化

                    fixed (float* fourier = spectrums)
                    {
                        ComputePowerSpectrum(outp, fourier, windowed.Length);
                    }
                }
                    
            }

            return spectrums;   // パワースペクトルを返す
        }
    }
}
