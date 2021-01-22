using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SpectrumObjectScript : MonoBehaviour
{
    public int SampleN { get; set; }
    public float[] Spectrums { get; set; }
    public float Aspect { get; set; }

    Transform ts;

    public RecordManager Recorder { get; set; }

    LineRenderer line;
    float xdiff;
    float offset;
    int uniqueSamples;
    bool isInitialized = false;

    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();
        SampleN = 1;
        ts = transform;
    }

    public void Setup(int sampleN)
    {
        SampleN = sampleN;
        uniqueSamples = sampleN >> 1;
        xdiff = Aspect * 2f / (float)(uniqueSamples);
        offset = Aspect;
        Spectrums = new float[sampleN];
        line.positionCount = uniqueSamples;
        isInitialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnRenderObject()
    {
        if (Recorder == null)
        {
            isInitialized = false;
            return;
        }
        if (!Recorder.IsRecording)
        {
            isInitialized = false;
            return;
        }

        float maximum = 1f / Mathf.Max(Spectrums);

        for (int i = 0; i < uniqueSamples; ++i)
        {
            line.SetPosition(i, 
                new Vector3(
                    (float)i * xdiff - offset,
                    Spectrums[i] * maximum,
                    0f));
        }
    }
}
