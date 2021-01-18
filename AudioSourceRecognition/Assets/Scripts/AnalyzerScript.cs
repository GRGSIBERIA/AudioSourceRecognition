using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnalyzerScript : MonoBehaviour
{
    [SerializeField]
    GameObject recordObject;

    RecordManager recorder;

    [SerializeField]
    int startSample = 1024;

    [SerializeField]
    int fourierCount = 7;

    float[][] fourier;

    // Start is called before the first frame update
    void Start()
    {
        recorder = recordObject.GetComponent<RecordManager>();

        fourier = new float[fourierCount][];
        for (int i = 0; i < fourierCount; ++i)
        {
            fourier[i] = new float[startSample << i];
        }
    }

    // Update is called once per frame
    void Update()
    {
        var sound = recorder.Source;

        if (sound == null) return;  // 未割り当ての場合はここから先の処理は行わない

        var buffer = recorder.GetData();

        for (int i = 0; i < fourierCount; ++i)
        {
            // フーリエのバッファに蓄えさせる
            System.Array.Copy(buffer, buffer.Length - fourier[i].Length - 1, fourier[i], 0, fourier[i].Length);
        }
    }
}
