using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExternalLibrary;

public class AnalyzerScript : MonoBehaviour
{
    [SerializeField]
    GameObject recordObject;

    RecordManager recorder;

    [SerializeField]
    int startSample = 1024;

    [SerializeField]
    int fourierCount = 7;

    BlackmanHarrisWindow[] windows;

    FourierTransform[] fft;

    void InitializeWindow()
    {
        windows = new BlackmanHarrisWindow[fourierCount];
        fft = new FourierTransform[fourierCount];
        for (int i = 0; i < fourierCount; ++i)
        {
            windows[i] = new BlackmanHarrisWindow(startSample << i, recorder.SamplingRate);
            fft[i] = new FourierTransform(startSample << i, windows[i]);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        recorder = recordObject.GetComponent<RecordManager>();
    }

    public void InvokeAnalyze()
    {
        InitializeWindow();
    }

    // Update is called once per frame
    void Update()
    {
        var sound = recorder.Source;

        // 未割り当て、もしくはレコーディング中でない場合はここから先の処理は行わない
        if (sound.clip == null || !recorder.IsRecording) return;

        var buffer = recorder.GetData();

        for (int i = 0; i < fourierCount; ++i)
        {
            // フーリエのバッファに蓄えさせる
            fft[i].FFT(buffer);
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < fourierCount; ++i)
        {
            windows[i].Dispose();
            fft[i].Dispose();
        }
    }
}


