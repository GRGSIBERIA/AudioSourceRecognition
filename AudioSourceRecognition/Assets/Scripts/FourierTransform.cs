using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using System;

namespace ExternalLibrary
{
    /// <summary>
    /// Stockhamアルゴリズムで実装したフーリエ変換
    /// </summary>
    public class FourierTransform : IDisposable
    {
        int N;
        NativeArray<float2> input;
        NativeArray<float2> output;

        NativeArray<float> spectrums;
        AbstractWindow wf;
        
        /// <summary>
        /// フーリエ変換
        /// </summary>
        /// <param name="N">サンプル点数</param>
        /// <param name="windowFunc">窓関数</param>
        public FourierTransform(int N, AbstractWindow wf)
        {
            this.N = N;
            input = new NativeArray<float2>(N, Allocator.Persistent);
            output = new NativeArray<float2>(N, Allocator.Persistent);
            spectrums = new NativeArray<float>(N, Allocator.Persistent);
            this.wf = wf;
        }

        float2 Cross(float2 a, float2 b)
        {
            return new float2(a.x * b.x - a.y * b.y, a.x * b.y + a.y * b.x);
        }

        void FFT0(int n, int s, bool eo, NativeArray<float2> x, NativeArray<float2> y)
        {
            int m = n >> 1;
            float theta0 = 2f * Mathf.PI / n;

            if (N == 2)
            {
                NativeArray<float2> z = eo ? y : x;
                for (int q = 0; q < s; ++q)
                {
                    float2 a = x[q + 0];
                    float2 b = x[q + s];
                    z[q + 0] = a + b;
                    z[q + s] = a - b;
                }
            }
            else if (n >= 4)
            {
                for (int p = 0; p < m; ++p)
                {
                    float2 wp = new float2(Mathf.Cos(p * theta0), -Mathf.Sin(p * theta0));

                    for (int q = 0; q < s; ++q)
                    {
                        float2 a = x[q + s * (p + 0)];
                        float2 b = x[q + s * (p + m)];
                        y[q + s * (2 * p + 0)] = a + b;
                        y[q + s * (2 * p + 1)] = Cross(a - b, wp);
                    }
                }
                FFT0(n >> 1, s << 1, !eo, y, x);
            }
        }

        /// <summary>
        /// 初期化ハンドル
        /// </summary>
        JobHandle handles;

        [BurstCompile]
        struct InitializeJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float> windowed;
            [WriteOnly] public NativeArray<float2> input;
            [WriteOnly] public NativeArray<float2> output;

            void IJobParallelFor.Execute(int i)
            {
                input[i] = new float2(windowed[i], 0f);
                output[i] = float2.zero;
            }
        }

        [BurstCompile]
        struct FinalizeJob : IJobParallelFor
        {
            [ReadOnly] public int N;
            [ReadOnly] public NativeArray<float2> input;
            [WriteOnly] public NativeArray<float> spectrums;

            public void Execute(int i)
            {
                float2 INP = input[i] / N;
                
                spectrums[i] =
                    Mathf.Sqrt(
                        INP.x * INP.x +
                        INP.y * INP.y);
            }
        }

        public NativeArray<float> FFT(float[] waveform)
        {
            NativeArray<float> windowed = wf.UseWindowing(waveform, handles);

            //---------------------------------------------------
            {   // 初期化フェーズ
                handles.Complete();
                InitializeJob initjob = new InitializeJob()
                {
                    windowed = windowed,
                    input = this.input,
                    output = this.output
                };
                handles = initjob.Schedule(N, 256);
                JobHandle.ScheduleBatchedJobs();
                handles.Complete();
            }

            //---------------------------------------------------
            // FFTの実行
            FFT0(N, 1, false, input, output);

            //---------------------------------------------------
            {   // 出力フェーズ
                handles.Complete();
                FinalizeJob finalizejob = new FinalizeJob()
                {
                    N = this.N,
                    input = this.input,
                    spectrums = this.spectrums
                };
                handles = finalizejob.Schedule(N, 256);
                JobHandle.ScheduleBatchedJobs();
                handles.Complete();
            }

            return spectrums;
        }

        public void Dispose()
        {
            input.Dispose();
            output.Dispose();
            spectrums.Dispose();
        }
    }
}
