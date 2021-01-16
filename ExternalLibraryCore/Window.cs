using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace ExternalLibraryCore
{
    public interface IWindow
    {
        float[] UseWindowing(float[] waveform, int spfreq);
    }

    public delegate float WindowFunction(float time);

    public abstract class AbstractWindow : IWindow
    {
        /// <summary>
        /// AVXは256個の数字を
        /// </summary>
        const int avx = 8;

        private int length;
        private int spfreq;
        private int block;
        private float diff;
        private float time;

        private float[] buffer;
        private float[] window;
        private float[] windowed;

        /// <summary>
        /// 窓掛け後の長さを返す
        /// </summary>
        public int Length { get { return length; } }

        public abstract float[] UseWindowing(float[] waveform, int spfreq);

        void Initialize(int N, int spfreq)
        {
            this.spfreq = spfreq;
            int logN = (int)MathF.Log2(N);
            int powN = (int)MathF.Pow(2f, logN);
            this.block = powN / avx;
            this.length = powN;

            this.buffer = new float[this.length];
            this.window = new float[this.length];
            this.windowed = new float[this.length];

            this.diff = 1f / (float)spfreq;
            this.time = this.diff * this.window.Length;
        }

        protected void CheckLength(float[] waveform, int spfreq)
        {
            // waveformの長さと内部的に保持している長さは一致しない
            if (waveform.Length < this.length || this.spfreq != spfreq)
            {
                Initialize(waveform.Length, spfreq);
            }
        }

        protected void CreateWindow(WindowFunction wf)
        {
            for (int i = 0; i < this.length; ++i)
            {
                window[i] = wf(i * this.diff / this.time);
            }
        }

        protected float[] Windowing(float[] waveform, int spfreq)
        {
            CheckLength(waveform, spfreq);

            // N点のバッファにコピー
            Array.Copy(waveform, waveform.Length - this.length, buffer, 0, this.length);

            Vector256<float> wavein;
            Vector256<float> output;

            unsafe
            {
                for (int i = 0; i < this.length; i += avx)
                {
                    fixed (float* pwavein = &buffer[i])
                    {
                        wavein = Avx.LoadVector256(pwavein);
                    }
                    fixed (float* pwinin = &window[i])
                    {
                        output = Avx.LoadVector256(pwinin);
                    }

                    output = Avx.Multiply(wavein, output);

                    fixed (float* pout = &windowed[i])
                    {
                        Avx.Store(pout, output);
                    }
                }
            }
            return windowed;
        }

        public AbstractWindow(int N,
                                 int samplingFrequency,
                                 WindowFunction wf)
        {
            Initialize(N, samplingFrequency);
            CreateWindow(wf);
        }
    }

    /// <summary>
    /// ハン窓
    /// </summary>
    public class HannWindow : AbstractWindow
    {
        /// <summary>
        /// ハン窓
        /// </summary>
        /// <param name="N">波形から切り出すサンプル数</param>
        /// <param name="samplingFrequency">サンプリング周波数</param>
        /// <remarks>サンプル数はN=2^Mが保証される</remarks>
        public HannWindow(int N, int samplingFrequency)
            : base(N, samplingFrequency, Hann) { }

        /// <summary>
        /// 波形に窓関数を適用する
        /// </summary>
        /// <param name="waveform">波形データ</param>
        /// <param name="spfreq">サンプリング周波数</param>
        /// <returns>窓かけした波形データ</returns>
        public override float[] UseWindowing(float[] waveform, int spfreq)
        {
            return Windowing(waveform, spfreq);
        }

        static float Hann(float t)
        {
            return 0.5f - 0.5f * MathF.Cos(2f * MathF.PI * t);
        }
    }

    /// <summary>
    /// ハミング窓
    /// </summary>
    public class HammingWindow : AbstractWindow
    {
        /// <summary>
        /// ハミング窓
        /// </summary>
        /// <param name="N">波形から切り出すサンプル数</param>
        /// <param name="samplingFrequency">サンプリング周波数</param>
        /// <remarks>サンプル数はN=2^Mが保証される</remarks>
        public HammingWindow(int N, int samplingFrequency)
            : base(N, samplingFrequency, Hamming) { }

        /// <summary>
        /// 波形に窓関数を適用する
        /// </summary>
        /// <param name="waveform">波形データ</param>
        /// <param name="spfreq">サンプリング周波数</param>
        /// <returns>窓かけした波形データ</returns>
        public override float[] UseWindowing(float[] waveform, int spfreq)
        {
            return Windowing(waveform, spfreq);
        }

        static float Hamming(float t)
        {
            return 0.54f - 0.46f * MathF.Cos(2f * MathF.PI * t);
        }
    }

    /// <summary>
    /// 矩形窓
    /// </summary>
    public class RectangleWindow : AbstractWindow
    {
        /// <summary>
        /// 矩形窓
        /// </summary>
        /// <param name="N">波形から切り出すサンプル数</param>
        /// <param name="samplingFrequency">サンプリング周波数</param>
        /// <remarks>サンプル数はN=2^Mが保証される</remarks>
        public RectangleWindow(int N, int samplingFrequency)
            : base(N, samplingFrequency, Rect) { }

        /// <summary>
        /// 波形に窓関数を適用する
        /// </summary>
        /// <param name="waveform">波形データ</param>
        /// <param name="spfreq">サンプリング周波数</param>
        /// <returns>窓かけした波形データ</returns>
        public override float[] UseWindowing(float[] waveform, int spfreq)
        {
            return Windowing(waveform, spfreq);
        }

        static float Rect(float t)
        {
            return 1f;
        }
    }

    /// <summary>
    /// ブラックマン窓
    /// </summary>
    public class BlackmanWindow : AbstractWindow
    {
        /// <summary>
        /// ブラックマン窓
        /// </summary>
        /// <param name="N">波形から切り出すサンプル数</param>
        /// <param name="samplingFrequency">サンプリング周波数</param>
        /// <remarks>サンプル数はN=2^Mが保証される</remarks>
        public BlackmanWindow(int N, int samplingFrequency)
            : base(N, samplingFrequency, Blackman) { }

        /// <summary>
        /// 波形に窓関数を適用する
        /// </summary>
        /// <param name="waveform">波形データ</param>
        /// <param name="spfreq">サンプリング周波数</param>
        /// <returns>窓かけした波形データ</returns>
        public override float[] UseWindowing(float[] waveform, int spfreq)
        {
            return Windowing(waveform, spfreq);
        }

        static float Blackman(float t)
        {
            return 0.42f - 
                0.5f * MathF.Cos(2f * MathF.PI * t) + 
                0.08f * MathF.Cos(4f * MathF.PI * t);
        }
    }

    /// <summary>
    /// ブラックマン-ハリス窓
    /// </summary>
    public class BlackmanHarrisWindow : AbstractWindow
    {
        /// <summary>
        /// ブラックマン-ハリス窓
        /// </summary>
        /// <param name="N">波形から切り出すサンプル数</param>
        /// <param name="samplingFrequency">サンプリング周波数</param>
        /// <remarks>サンプル数はN=2^Mが保証される</remarks>
        public BlackmanHarrisWindow(int N, int samplingFrequency)
            : base(N, samplingFrequency, BlackmanHarris) { }

        /// <summary>
        /// 波形に窓関数を適用する
        /// </summary>
        /// <param name="waveform">波形データ</param>
        /// <param name="spfreq">サンプリング周波数</param>
        /// <returns>窓かけした波形データ</returns>
        public override float[] UseWindowing(float[] waveform, int spfreq)
        {
            return Windowing(waveform, spfreq);
        }

        static float BlackmanHarris(float t)
        {
            return 0.355768f - 
                0.487396f * MathF.Cos(2f * MathF.PI * t) + 
                0.144232f * MathF.Cos(4f * MathF.PI * t) -
                0.012604f * MathF.Cos(6f * MathF.PI * t);
        }
    }
}
