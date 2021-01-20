using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExternalLibrary;
using Unity.Collections;

public class AnalyzerScript : MonoBehaviour
{
    [SerializeField]
    GameObject recordObject;

    [SerializeField]
    GameObject fourierPrefab;

    [SerializeField]
    int sampleN = 8192;

    float aspect = 6f;

    float[] spectrums;

    RecordManager recorder;

    Transform ts;

    void InitializeWindow()
    {
        spectrums = new float[sampleN];
    }

    // Start is called before the first frame update
    void Start()
    {
        ts = GetComponent<Transform>();
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

        sound.GetSpectrumData(spectrums, 1, FFTWindow.BlackmanHarris);

        GameObject inst = Instantiate(fourierPrefab, ts);
        var fourier = inst.GetComponent<SpectrumObjectScript>();
        fourier.SampleN = sampleN;
        fourier.Spectrums = new float[sampleN];
        fourier.Aspect = aspect;
        System.Array.Copy(spectrums, 0, fourier.Spectrums, 0, sampleN);
    }
}


