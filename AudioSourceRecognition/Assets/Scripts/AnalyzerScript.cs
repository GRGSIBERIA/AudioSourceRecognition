using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExternalLibrary;
using Unity.Collections;
using System;

public class AnalyzerScript : MonoBehaviour
{
    [SerializeField]
    GameObject recordObject;

    [SerializeField]
    GameObject fftSampleObject;

    [SerializeField]
    GameObject fourierPrefab;

    int sampleN = 8192;

    float aspect = 6f;

    float[] spectrums;
    float[] pastSpectrums;

    RecordManager recorder;

    SpectrumObjectScript fourier;

    Transform ts;

    AbstractWindow wf;
    FourierTransform fft;


    void InitializeWindow()
    {
        spectrums = new float[sampleN];
        pastSpectrums = new float[sampleN];
        wf = new HammingWindow(sampleN, recorder.SamplingRate);
        fft = new FourierTransform(sampleN, wf);
    }

    // Start is called before the first frame update
    void Start()
    {
        ts = GetComponent<Transform>();
        recorder = recordObject.GetComponent<RecordManager>();
        var fftSample = fftSampleObject.GetComponent<InputField>();

        if (!int.TryParse(fftSample.text, out sampleN))
        {
            throw new ArgumentException("Invalid fft sample text.");
        }

        GameObject inst = Instantiate(fourierPrefab, ts);
        fourier = inst.GetComponent<SpectrumObjectScript>();
        fourier.SampleN = sampleN;
        fourier.Spectrums = new float[sampleN];
        fourier.Aspect = aspect;
        fourier.Recorder = recorder;
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

        float[] data = recorder.GetData();
        fft.FFT(data).CopyTo(spectrums);
        
        // ここで過去のデータにする
        Array.Copy(spectrums, 0, fourier.Spectrums, 0, sampleN);
        Array.Copy(spectrums, 0, pastSpectrums, 0, sampleN);
    }

    private void OnDestroy()
    {
        if (wf != null) wf.Dispose();
        if (fft != null) fft.Dispose();
    }
}


