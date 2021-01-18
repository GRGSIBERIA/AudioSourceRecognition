using System;
using System.IO;
using ExternalLibrary;

namespace OutputTest
{
    class Program
    {
        static void Main(string[] args)
        {
            const int SamplingFrequency = 44100;
            const float Time = 2.0f;
            const float Frequency = 441.0f;
            const int N = 1024;

            const float dt = 1f / SamplingFrequency;
            const int NumberOfSamples = (int)(Time / dt);

            float[] waveform = new float[NumberOfSamples];
            for (int i = 0; i < NumberOfSamples; ++i)
            {
                waveform[i] = MathF.Sin(2f * MathF.PI * Frequency * dt * i);
            }

            using (StreamWriter w = new StreamWriter("./SinWave.txt"))
            {
                for (int i = 0; i < NumberOfSamples; ++i)
                {
                    w.WriteLine($"{dt * i},{waveform[i]}");
                }
            }

            BlackmanHarrisWindow window = new BlackmanHarrisWindow(1024, SamplingFrequency);
            FourierTransform fft = new FourierTransform(window, SamplingFrequency);

            float[] output = fft.FFT(waveform);

            using (StreamWriter w = new StreamWriter("./Fourier.txt"))
            {
                for (int i = 0; i < output.Length; ++i)
                {
                    w.WriteLine($"{i},{output[i]}");
                }
            }
        }
    }
}
