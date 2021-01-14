using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WaveformScript : MonoBehaviour
{
    [SerializeField]
    GameObject recordManager;

    LineRenderer line;
    RecordManager recorder;

    Vector3[] vertices;

    bool initialized = false;

    int pvWidth;
    int pvHeight;

    // Start is called before the first frame update
    void Start()
    {
        recorder = recordManager.GetComponent<RecordManager>();
        line = GetComponent<LineRenderer>();
        line.startWidth = 0.01f;
        line.endWidth = 0.01f;
        pvWidth = Screen.width;
        pvHeight = Screen.height;
        CleanAspect(pvWidth, pvHeight);
    }

    private void Update()
    {
        if (recorder.IsRecording && !initialized)
        {
            vertices = new Vector3[recorder.NumofSamples];
            line.positionCount = vertices.Length;
            initialized = true;
        }
    }

    [SerializeField]
    float aspectX;

    [SerializeField]
    float aspectY;

    void CleanAspect(int width, int height)
    {
        aspectX = 5;
        aspectY = 5;
    }

    // Update is called once per frame
    public void OnRenderObject()
    {
        var resolution = Screen.currentResolution;

        if (!recorder.IsRecording) return;

        var waveform = recorder.GetData();

        // アスペクト比は固定で行く

        float offset = aspectX;
        float xdiff = aspectX * 2f / (float)waveform.Length;
        float ydiff = 0.5f;

        for (int i = 0; i < waveform.Length; ++i)
        {
            vertices[i].x = i * xdiff - offset;
            vertices[i].y = waveform[i] * ydiff;
            vertices[i].z = 0f;
        }

        line.SetPositions(vertices);

    }
}
