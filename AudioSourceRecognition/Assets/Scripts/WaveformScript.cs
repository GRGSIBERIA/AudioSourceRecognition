using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

[RequireComponent(typeof(LineRenderer))]
public class WaveformScript : MonoBehaviour
{
    [SerializeField]
    GameObject recordManager;

    LineRenderer line;
    RecordManager recorder;

    Vector3[] vertices;

    bool initialized = false;

    [SerializeField]
    float aspect;

    // Start is called before the first frame update
    void Start()
    {
        recorder = recordManager.GetComponent<RecordManager>();
        line = GetComponent<LineRenderer>();
        line.startWidth = 0.01f;
        line.endWidth = 0.01f;
        aspect = 6f;    // 4:3Ç≈Ç‡óêÇÍÇ»Ç¢íl
    }

    private void Update()
    {
        if (recorder.IsRecording && !initialized)
        {
            vertices = new Vector3[recorder.NumofSamples];
            line.positionCount = vertices.Length;
            initialized = true;

            float offset = aspect;
            float xdiff = aspect * 2f / (float)vertices.Length;

            for (int i = 0; i < vertices.Length; ++i)
            {
                vertices[i].x = i * xdiff - offset;
                vertices[i].z = 0f;
            }
        }
        else if (!recorder.IsRecording && initialized)
        {
            initialized = false;
        }
    }

    // Update is called once per frame
    public void OnRenderObject()
    {
        var resolution = Screen.currentResolution;

        if (!recorder.IsRecording) return;

        var waveform = recorder.GetData();

        float ydiff = 0.5f;
        for (int i = 0; i < waveform.Length; ++i)
        {
            vertices[i].y = waveform[i] * ydiff;
        }

        line.SetPositions(vertices);
    }
}
