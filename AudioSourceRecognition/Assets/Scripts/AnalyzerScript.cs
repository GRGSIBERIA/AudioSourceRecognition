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

    void InitializeWindow()
    {
        windows = new BlackmanHarrisWindow[fourierCount];
        for (int i = 0; i < fourierCount; ++i)
        {
            windows[i] = new BlackmanHarrisWindow(startSample << i, recorder.SamplingRate);
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

        if (sound == null) return;  // 未割り当ての場合はここから先の処理は行わない

        var buffer = recorder.GetData();

        for (int i = 0; i < fourierCount; ++i)
        {
            // フーリエのバッファに蓄えさせる
            
        }
    }
}


