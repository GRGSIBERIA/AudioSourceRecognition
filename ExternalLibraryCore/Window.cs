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

        private float[] window;
        private float[] windowed;

        public abstract float[] UseWindowing(float[] waveform, int spfreq);

        void Initialize(int length, int spfreq)
        {
            this.spfreq = spfreq;
            this.block = length / avx;
            this.length = this.block * avx;
            this.window = new float[this.length];
            this.windowed = new float[this.length];

            this.diff = 1f / (float)spfreq;
            this.time = this.diff * this.window.Length;
        }

        protected void CheckLength(float[] waveform, int spfreq)
        {
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

            Vector256<float> wavein;
            Vector256<float> output;

            unsafe
            {
                for (int i = 0; i < this.length; i += avx)
                {
                    fixed (float* pwavein = &waveform[i])
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

        public AbstractWindow(int waveformLength,
                                 int samplingFrequency,
                                 WindowFunction wf)
        {
            Initialize(waveformLength, samplingFrequency);
            CreateWindow(wf);
        }
    }

    /// <summary>
    /// ハン窓
    /// </summary>
    public class HannWindow : AbstractWindow
    {
        public HannWindow(int waveformLength, int samplingFrequency)
            : base(waveformLength, samplingFrequency, Hann) { }

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
        /// <param name="waveformLength">波形のサンプル数</param>
        /// <param name="samplingFrequency">サンプリング周波数</param>
        public HammingWindow(int waveformLength, int samplingFrequency)
            : base(waveformLength, samplingFrequency, Hamming) { }

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
        /// <param name="waveformLength">波形のサンプル数</param>
        /// <param name="samplingFrequency">サンプリング周波数</param>
        public RectangleWindow(int waveformLength, int samplingFrequency)
            : base(waveformLength, samplingFrequency, Rect) { }

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
        /// <param name="waveformLength">波形のサンプル数</param>
        /// <param name="samplingFrequency">サンプリング周波数</param>
        public BlackmanWindow(int waveformLength, int samplingFrequency)
            : base(waveformLength, samplingFrequency, Blackman) { }

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
        /// <param name="waveformLength">波形のサンプル数</param>
        /// <param name="samplingFrequency">サンプリング周波数</param>
        public BlackmanHarrisWindow(int waveformLength, int samplingFrequency)
            : base(waveformLength, samplingFrequency, BlackmanHarris) { }

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
