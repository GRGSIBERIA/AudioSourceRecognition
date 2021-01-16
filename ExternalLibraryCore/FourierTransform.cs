using System;
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
            if (window.Length != N)
                throw new ArgumentException("Window size not equals N");

            this.window = window;
            this.length = N;
            this.spfreq = samplingFreq;

            // 実部は窓掛けしたものを代入する
            this.input = new complex_t[this.length];
            this.spectrums = new float[this.length / 2];
        }

        unsafe void fft0(int n, int s, bool eo, complex_t* x, complex_t* y)
        {
            int m = n >> 1;
            float theta0 = 2f * MathF.PI / n;

            if (n == 2)
            {
                
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
                Vector256<float> a, b, c, z;
                fixed (float* wp = &windowed[i])
                {
                    Sse.Prefetch0(wp + avx);
                    a = Avx.LoadVector256(wp);
                    z = Avx.LoadVector256(wp);
                    z = Avx.Xor(z, z);  // zeroベクトル
                }

                // a -- 0 1 2 3 4 5 6 7 load
                // z -- X X X X X X X X load xor
                // b -- 0 X 2 X 4 X 6 X unpacklow(a,z)
                // c -- X 1 X 3 X 5 X 7 unpackhi(a,z)

                b = Avx.UnpackLow(a, z);
                c = Avx.UnpackHigh(a, z);
                
                fixed (complex_t* cp = &input[i])
                {
                    Avx.Store(&cp->re, b);
                }
                fixed (complex_t* cp = &input[i + 4])
                {
                    Avx.Store(&cp->re, c);
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
