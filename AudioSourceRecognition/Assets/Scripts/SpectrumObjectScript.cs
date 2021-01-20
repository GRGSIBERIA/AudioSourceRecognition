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

    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = SampleN;
        pos = new Vector3[SampleN];

        xdiff = Aspect * 2f / (float)SampleN;
        offset = Aspect;

        ts = transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnRenderObject()
    {
        if (!Recorder.IsRecording) return;

        for (int i = 0; i < SampleN; ++i)
        {
            if (!(float.IsInfinity(Spectrums[i]) || float.IsNaN(Spectrums[i])))
            {
                pos[i] = new Vector3(
                (float)i * xdiff - offset,
                Mathf.Log(Spectrums[i]),
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
