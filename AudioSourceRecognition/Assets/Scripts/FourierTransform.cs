using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;



public class FourierTransform
{
    int sampleSize;
    float2[] complexies;
    float[] outputs;

    public FourierTransform(int sampleSize)
    {
        this.sampleSize = sampleSize;
        complexies = new float2[sampleSize];
        outputs = new float[sampleSize];
    }

    public float[] FFT(float[] inputs)
    {
        if (inputs.Length < sampleSize)
        {
            Debug.LogError($"Over sampling the number of inputs {inputs.Length} vs the number of sample size {sampleSize}");
            throw new UnityEngine.UnityException("You gives the input time line less than sample size.");
        }

        return outputs;
    }
}
