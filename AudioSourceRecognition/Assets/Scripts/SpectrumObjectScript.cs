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

    Vector3[] pos;

    public RecordManager Recorder { get; set; }

    LineRenderer line;
    float xdiff;
    float offset;
    int uniqueSamples;

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
        xdiff = Aspect * 2f / (float)(SampleN >> 1);
        offset = Aspect;
        Spectrums = new float[sampleN];
        pos = new Vector3[uniqueSamples];
        line.positionCount = uniqueSamples;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnRenderObject()
    {
        if (!Recorder.IsRecording) return;

        float maximum = 1f / Mathf.Max(Spectrums);

        for (int i = 0; i < uniqueSamples; ++i)
        {
            if (!(float.IsInfinity(Spectrums[i]) || float.IsNaN(Spectrums[i])))
            {
                pos[i] = new Vector3(
                (float)i * xdiff - offset,
                Spectrums[i] * maximum,
                0f);
            }
            else
            {
                pos[i] = new Vector3(
                    (float)i * xdiff - offset,
                    0f,
                    0f);
            }
        }
        line.SetPositions(pos);
    }
}
