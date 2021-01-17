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

            // スペクトルは半分
            this.spectrums = new float[this.length >> 1];

            // thetaテーブル
            this.theta0_table = new Dictionary<int, float>();
            for (int n = this.length; n != 2; n = n >> 1)
            {
                theta0_table.Add(n, 2f * MathF.PI / n);
            }
        }

        private unsafe void fft0(int n, int s, bool eo, complex_t* x, complex_t* y)
        {
            int m = n >> 1;
            float theta0 = theta0_table[n];

            if (n == 2)
            {
                // ストライドが2倍ずつ増えているのに、4以下になることは考えない
                complex_t* z = eo ? y : x;
                for (int q = 0; q < s; q += 4)
                {
                    Vector256<float> a, b;
                    float* xf = &(x + q)->re;
                    float* zf = &(z + q)->re;

                    a = Avx.LoadVector256(xf);
                    b = Avx.LoadVector256(xf + 4 * s);
                    Avx.Store(zf, Avx.Add(a, b));
                    Avx.Store(zf + 4 * s, Avx.Subtract(a, b));
                }
            }
            else if (n >= 4)
            {
                if (s == 1) // 1番最初に実行されたとき
                {

                }
                else        // 2回目以降に実行されたとき
                {
                    for (int p = 0; p < m; ++p)
                    {
                        // サインコサインのテーブルを作る
                        Vector256<float> wp;
                        worker[0] = MathF.Cos(p * theta0);
                        worker[1] = -MathF.Sin(p * theta0);
                        for (int i = 1; i < 8; ++i)
                        {
                            worker[i] = (i & 1) == 0 ? worker[0] : worker[1];
                        }   // シャッフルするよりコピーしたほうが速いかもしれない
                        fixed (float* workerp = worker)
                            wp = Avx.LoadVector256(workerp);

                        Vector256<float> a, b, yy;
                        for (int q = 0; q < s; q += 4)
                        {
                            /*
                            complex_t* a = &x[q + s * (p + 0)];
                            complex_t* b = &x[q + s * (p + m)];
                            y[q + s * (2 * p + 0)].re = a->re + b->re;
                            y[q + s * (2 * p + 0)].im = a->im + b->im;
                            temp.re = a->re - b->re;
                            temp.im = a->re - b->im;
                            y[q + s * (2 * p + 1)].re = temp.re * wp.re - temp.im * wp.im;
                            y[q + s * (2 * p + 1)].im = temp.re * wp.im + temp.im * wp.re;
                            */

                            float* x0 = &x[q + s * (p + 0)].re;
                            float* xm = &x[q + s * (p + m)].re;

                            float* y1p = &y[q + s * (2 * p + 0)].re;
                            float* y2p = &y[q + s * (2 * p + 1)].re;

                            a = Avx.LoadVector256(x0);
                            b = Avx.LoadVector256(xm);
                            yy = Avx.Add(a, b);
                            Avx.Store(y1p, yy); // 1個目の演算

                            yy = Avx.Subtract(a, b);
                            Vector256<float> yx;
                            a = Avx.UnpackLow(wp, wp);
                            b = Avx.UnpackHigh(wp, wp);
                            yx = Avx.Shuffle(yy, yy, 0x55);
                            yx = Avx.AddSubtract(Avx.Multiply(a, yy), Avx.Multiply(b, yx));
                            Avx.Store(y2p, yx); // 2個目の演算
                        }
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

        /// <summary>
        /// 高速フーリエ変換
        /// </summary>
        /// <param name="waveform">波形</param>
        /// <returns></returns>
        public float[] FFT(float[] waveform)
        {
            float[] windowed = window.UseWindowing(waveform, this.spfreq);

            InitializeInputComplex(windowed);
            unsafe
            {
                fixed (complex_t* inp = input)
                {
                    fixed (complex_t* outp = output)
                    {
                        fft0(windowed.Length, 1, false, inp, outp);
                        InvN(outp, windowed.Length);
                    }
                }
                    
            }

            return spectrums;
        }
    }
}
