using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace ExternalLibraryCore
{
    public interface IWindow
    {
        float[] UseWindowing(float[] waveform, int length);
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

        public abstract float[] UseWindowing(float[] waveform, int length);

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
            if (waveform.Length != this.length || this.spfreq != spfreq)
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

        protected float[] Windowing(float[] waveform, int length)
        {
            CheckLength(waveform, length);

            Vector256<float> wavein;
            Vector256<float> winin;
            Vector256<float> output;

            unsafe
            {
                for (int i = 0; i < this.length; i += avx)
                {
                    fixed (float* pwavein = &waveform[i])
                    {
                        wavein = Avx.LoadAlignedVector256(pwavein);
                    }
                    fixed (float* pwinin = &window[i])
                    {
                        winin = Avx.LoadAlignedVector256(pwinin);
                    }

                    output = Avx.Multiply(wavein, winin);

                    fixed (float* pout = &windowed[i])
                    {
                        Avx.StoreAligned(pout, output);
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

    public class HannWindow : AbstractWindow
    {
        public HannWindow(int waveformLength, int samplingFrequency)
            : base(waveformLength, samplingFrequency, Hann) { }

        public override float[] UseWindowing(float[] waveform, int length)
        {
            return Windowing(waveform, length);
        }

        static float Hann(float t)
        {
            return 0.5f - 0.5f * MathF.Cos(2f * MathF.PI * t);
        }
    }

    public class HammingWindow : AbstractWindow
    {
        public HammingWindow(int waveformLength, int samplingFrequency)
            : base(waveformLength, samplingFrequency, Hamming) { }

        public override float[] UseWindowing(float[] waveform, int length)
        {
            return Windowing(waveform, length);
        }

        static float Hamming(float t)
        {
            return 0.54f - 0.46f * MathF.Cos(2f * MathF.PI * t);
        }
    }
}
