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

        complex_t[] input;
        float[] spectrums;
        AbstractWindow window;

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
            this.spectrums = new float[this.length / 2];

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
                if (s == 1)
                {
                    float* xs = &x->re;
                    float* zs = &z->re;
                    Vector128<float> a = Sse.LoadVector128(xs + 2 * 0);
                    Vector128<float> b = Sse.LoadVector128(xs + 2 * 1);
                    Sse.Store(zs + 2 * 0, Sse.Add(a, b));
                    Sse.Store(zs + 2 * 1, Sse.Subtract(a, b));
                }
                else
                {
                    for (int q = 0; q < s; q += 2)
                    {
                        float* xs = &(x + q)->re;
                        float* zs = &(z + q)->re;
                        Vector256<float> a = Avx.LoadVector256(xs + 2 * 0);
                        Vector256<float> b = Avx.LoadVector256(xs + 2 * s);
                        Avx.Store(zs + 2 * 0, Avx.Add(a, b));
                        Avx.Store(zs + 2 * s, Avx.Subtract(a, b));
                    }
                }
            }
            else if (n >= 4)
            {
                if (s == 1)
                {
                    
                }
                else
                {

                }
                fft0(n >> 1, s << 1, !eo, y, x);
            }
        }
        
        unsafe void InitializeInputComplex(float[] windowed)
        {
            for (int i = 0; i < this.length; i += avx)
            {
                Vector256<float> a, z;
                fixed (float* wp = &windowed[i])
                {
                    Sse.Prefetch0(wp + avx);
                    a = Avx.LoadVector256(wp);
                    z = Avx.Xor(a, a);  // zeroベクトル
                }

                // a -- 0 1 2 3 4 5 6 7 load
                // z -- X X X X X X X X xor(a,a)
                // z -- 0 X 1 X 2 X 3 X unpacklps(a,z)
                // z -- X X X X X X X X xor(z,z)
                // z -- 4 X 5 X 6 X 7 X unpackhps(a,z)
                
                // 各々の複素数に代入
                fixed (complex_t* cp = &input[i])
                {
                    z = Avx.UnpackLow(a, z);
                    Avx.Store(&cp->re, z);
                    Avx.Xor(z, z);
                }
                fixed (complex_t* cp = &input[i + avx >> 1])
                {
                    z = Avx.UnpackHigh(a, z);
                    Avx.Store(&cp->re, z);
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
