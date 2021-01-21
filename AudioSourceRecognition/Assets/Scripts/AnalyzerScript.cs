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
    GameObject fftWindowShiftTimeObject;

    [SerializeField]
    GameObject fourierPrefab;

    int sampleN = 8192;
    int windowShiftTime = 1;
    int shiftSample;    // 1シフトごとのサンプル点数

    float aspect = 6f;

    
    float[][] shiftData;
    NativeArray<float>[] shiftSpectrums;
    float[] spectrums;
    float[] pastSpectrums;

    RecordManager recorder;

    SpectrumObjectScript fourier;

    Transform ts;

    AbstractWindow[] wf;
    FourierTransform[] fft;


    void InitializeWindow()
    {
        // sampleNの数をここで合わせる
        var fftSample = fftSampleObject.GetComponent<InputField>();
        var windowShift = fftWindowShiftTimeObject.GetComponent<InputField>();

        int power;
        if (!int.TryParse(fftSample.text, out power))
        {
            throw new ArgumentException("Invalid fft sample text.");
        }
        if (!int.TryParse(windowShift.text, out windowShiftTime))
        {
            throw new ArgumentException("Invalid window shift time.");
        }
        sampleN = 1 << power; // 2^Nであるかを聞いているから、その答えをここに

        spectrums = new float[sampleN];
        pastSpectrums = new float[sampleN];

        wf = new AbstractWindow[windowShiftTime];// (sampleN, recorder.SamplingRate);
        fft = new FourierTransform[windowShiftTime]; //(sampleN, wf);

        shiftData = new float[windowShiftTime][];
        shiftSpectrums = new NativeArray<float>[windowShiftTime];

        for (int i = 0; i < windowShiftTime; ++i)
        {
            wf[i] = new HannWindow(sampleN, recorder.SamplingRate);
            fft[i] = new FourierTransform(sampleN, wf[i]);
            shiftData[i] = new float[sampleN];
            //shiftSpectrums[i] = new float[sampleN];
        }
        
        shiftSample = sampleN / windowShiftTime;

        fourier.Setup(sampleN);     // スペクトログラムの初期化
    }

    // Start is called before the first frame update
    void Start()
    {
        ts = GetComponent<Transform>();
        recorder = recordObject.GetComponent<RecordManager>();
        
        GameObject inst = Instantiate(fourierPrefab, ts);
        fourier = inst.GetComponent<SpectrumObjectScript>();
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
        for (int i = 0; i < windowShiftTime; ++i)
            Array.Copy(data, data.Length - shiftSample * i - sampleN - 1, shiftData[i], 0, sampleN);

        for (int i = 0; i < windowShiftTime; ++i)
        {
            shiftSpectrums[i] = fft[i].FFT(shiftData[i]);
        }
        for (int i = 0; i < windowShiftTime; ++i)
        {
            for (int j = 0; j < sampleN; ++j)
                spectrums[j] += shiftSpectrums[i][j];
        }
        float inv = 1f / (float)windowShiftTime;
        for (int i = 0; i < sampleN; ++i)
            spectrums[i] *= inv;

        //fft.FFT(data).CopyTo(spectrums);

        Array.Copy(spectrums, 0, fourier.Spectrums, 0, sampleN);
        Array.Copy(spectrums, 0, pastSpectrums, 0, sampleN);
    }

    private void OnDestroy()
    {
        if (wf == null || fft == null) return;

        for (int i = 0; i < windowShiftTime; ++i)
        {
            wf[i].Dispose();
            fft[i].Dispose();
        }
    }
}


