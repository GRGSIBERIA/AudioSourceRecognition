using System;
using System.Collections;
using UnityEngine;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;

namespace ExternalLibrary
{
    public interface IWindow
    {
        NativeArray<float> UseWindowing(float[] waveform, JobHandle handles);
    }

    public delegate float WindowFunction(float time);

    public abstract class AbstractWindow : IWindow, IDisposable
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

        private NativeArray<float> buffer;
        private NativeArray<float> window;
        private NativeArray<float> windowed;

        const int batch = 512;

        /// <summary>
        /// 窓掛け後の長さを返す
        /// </summary>
        public int Length { get { return length; } }

        public abstract NativeArray<float> UseWindowing(float[] waveform, JobHandle handles);

        void Initialize(int N, int spfreq)
        {
            this.spfreq = spfreq;
            int logN = (int)Mathf.Log(N, 2f);
            int powN = (int)Mathf.Pow(2f, logN);

            if (powN != N) throw new ArgumentException($"N is not powered by 2^N : {powN}");

            this.block = powN / avx;
            this.length = powN;

            this.buffer = new NativeArray<float>(this.length, Allocator.Persistent);
            this.window = new NativeArray<float>(this.length, Allocator.Persistent);
            this.windowed = new NativeArray<float>(this.length, Allocator.Persistent);

            this.diff = 1f / (float)spfreq;
            this.time = this.diff * this.window.Length;
        }

        protected void CheckLength(float[] waveform)
        {
            // waveformの長さと内部的に保持している長さは一致しない
            if (waveform.Length < this.length)
            {
                //Initialize(waveform.Length, spfreq);
                throw new ArgumentException("waveform length less than N.");
            }
        }

        protected void CreateWindow(WindowFunction wf)
        {
            for (int i = 0; i < this.length; ++i)
            {
                window[i] = wf(i * this.diff / this.time);
            }
        }

        /// <summary>
        /// 窓掛け用のジョブ
        /// </summary>
        [BurstCompile]
        struct WindowingJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float> buffer;
            [ReadOnly] public NativeArray<float> window;
            [WriteOnly] public NativeArray<float> windowed;

            void IJobParallelFor.Execute(int i)
            {
                windowed[i] = window[i] * buffer[i];
            }
        }

        protected NativeArray<float> Windowing(float[] waveform, JobHandle handles)
        {
            CheckLength(waveform);

            // N点のバッファにコピー
            NativeArray<float>.Copy(waveform, waveform.Length - this.length, buffer, 0, this.length);

            // Burstで窓掛けのジョブを流す
            handles.Complete();
            WindowingJob job = new WindowingJob()
            {
                buffer = this.buffer,
                window = this.window,
                windowed = this.windowed
            };
            handles = job.Schedule(this.length, batch);
            JobHandle.ScheduleBatchedJobs();
            handles.Complete();

            return windowed;
        }

        public void Dispose()
        {
            this.buffer.Dispose();
            this.window.Dispose();
            this.windowed.Dispose();
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
        /// <returns>窓かけした波形データ</returns>
        public override NativeArray<float> UseWindowing(float[] waveform, JobHandle handles)
        {
            return Windowing(waveform, handles);
        }

        static float Hann(float t)
        {
            return 0.5f - 0.5f * Mathf.Cos(2f * Mathf.PI * t);
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
        /// <returns>窓かけした波形データ</returns>
        public override NativeArray<float> UseWindowing(float[] waveform, JobHandle handles)
        {
            return Windowing(waveform, handles);
        }

        static float Hamming(float t)
        {
            return 0.54f - 0.46f * Mathf.Cos(2f * Mathf.PI * t);
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
        /// <returns>窓かけした波形データ</returns>
        public override NativeArray<float> UseWindowing(float[] waveform, JobHandle handles)
        {
            return Windowing(waveform, handles);
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
        /// <returns>窓かけした波形データ</returns>
        public override NativeArray<float> UseWindowing(float[] waveform, JobHandle handles)
        {
            return Windowing(waveform, handles);
        }

        static float Blackman(float t)
        {
            return 0.42f -
                0.5f * Mathf.Cos(2f * Mathf.PI * t) +
                0.08f * Mathf.Cos(4f * Mathf.PI * t);
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
        /// <returns>窓かけした波形データ</returns>
        public override NativeArray<float> UseWindowing(float[] waveform, JobHandle handles)
        {
            return Windowing(waveform, handles);
        }

        static float BlackmanHarris(float t)
        {
            return 0.355768f -
                0.487396f * Mathf.Cos(2f * Mathf.PI * t) +
                0.144232f * Mathf.Cos(4f * Mathf.PI * t) -
                0.012604f * Mathf.Cos(6f * Mathf.PI * t);
        }
    }
}
