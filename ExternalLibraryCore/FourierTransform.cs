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
        float[] spectrums;      // スペクトル用の配列
        AbstractWindow window;  // 窓関数

        /// <summary>
        /// フーリエ変換
        /// </summary>
        /// <param name="window">窓関数</param>
        /// <param name="samplingFreq">サンプリング周波数</param>
        /// <param name="N">系列長</param>
        public FourierTransform(AbstractWindow window, int samplingFreq, int N)
        {
            // windowの窓幅とサンプル数は合致させなければならない
            if (window.Length != N)
                throw new ArgumentException("Window size not equals N");

            this.window = window;
            this.length = N;
            this.spfreq = samplingFreq;

            // 実部は窓掛けしたものを代入する
            this.input = new complex_t[this.length];

            // スペクトルは半分
            this.spectrums = new float[this.length >> 1];

            // thetaテーブル
            this.theta0_table = new Dictionary<int, float>();
            for (int n = N; n != 2; n = n >> 1)
            {
                theta0_table.Add(n, 2f * MathF.PI / n);
            }
        }

        unsafe void fft0(int n, int s, bool eo, complex_t* x, complex_t* y)
        {
            int m = n >> 1;
            float theta0 = theta0_table[n];

            if (n == 2)
            {
                complex_t* z = eo ? y : x;
                for (int q = 0; q < s; ++q)
                {
                    complex_t* a = &x[q + 0];
                    complex_t* b = &x[q + s];
                    z[q + 0].re = a->re + b->re;
                    z[q + 0].im = a->im + b->im;
                    z[q + s].re = a->re - b->re;
                    z[q + s].re = a->im - b->im;
                }
            }
            else if (n >= 4)
            {
                for (int p = 0; p < m; ++p)
                {
                    complex_t wp = new complex_t(
                        MathF.Cos(p * theta0), 
                        -MathF.Sin(p * theta0));

                    for (int q = 0; q < s; ++q)
                    {
                        complex_t* a = &x[q + s * (p + 0)];
                        complex_t* b = &x[q + s * (p + m)];
                        y[q + s * (2 * p + 0)].re = a->re + b->re;
                        y[q + s * (2 * p + 0)].im = a->im + b->im;
                        y[q + s * (2 * p + 1)].re = a->re * b->re - a->im * b->im;
                        y[q + s * (2 * p + 1)].im = a->re * b->im - a->im * b->re;
                    }
                }
                fft0(n >> 1, s << 1, !eo, y, x);
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
                // z -- X 3 X 2 unpackhi(a,z)
                
                fixed (complex_t* cp = &input[i])
                {
                    z = Sse.UnpackLow(a, z);
                    Sse.Store(&cp[0].re, z);
                    z = Sse.UnpackHigh(a, z);
                    Sse.Store(&cp[1].re, z);
                }
            }
        }

        /// <summary>
        /// 高速フーリエ変換
        /// </summary>
        /// <param name="waveform">波形</param>
        /// <returns></returns>
        public float[] FFT(float[] waveform)
        {
            float[] windowed = window.UseWindowing(waveform, this.spfreq);

            InitializeInputComplex(windowed);

            return spectrums;
        }
    }
}
