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

    Resolution prevResolution;

    // Start is called before the first frame update
    void Start()
    {
        recorder = recordManager.GetComponent<RecordManager>();
        line = GetComponent<LineRenderer>();
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
        prevResolution = Screen.currentResolution;
        CleanAspect(prevResolution);
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

    /// <summary>
    /// ユークリッド互除法
    /// </summary>
    /// <param name="x">長辺</param>
    /// <param name="y">短辺</param>
    /// <returns>公約数</returns>
    int gcd(int x, int y)
    {
        if (y == 0) return x;
        return gcd(y, x % y);
    }

    [SerializeField]
    float aspectX;

    [SerializeField]
    float aspectY;

    void CleanAspect(Resolution resolution)
    {
        float match = gcd(resolution.width, resolution.height);
        aspectX = resolution.width / match;
        aspectY = resolution.height / match;
    }

    // Update is called once per frame
    public void OnRenderObject()
    {
        var resolution = Screen.currentResolution;

        if (!recorder.IsRecording) return;

        var waveform = recorder.GetData();

        // 解像度に変更が加えられた場合の処理
        if (!(prevResolution.width == resolution.width && prevResolution.height == resolution.height))
        {
            CleanAspect(resolution);
        }

        float aspect = (float)resolution.width / (float)resolution.height;
        
        float offset = aspectY;
        float xdiff = aspectY * 2f / (float)waveform.Length;
        float ydiff = (float)aspectX * 0.25f / Mathf.Pow(2f, 24f);

        for (int i = 0; i < waveform.Length; ++i)
        {
            vertices[i].x = i * xdiff - offset;
            vertices[i].y = waveform[i] * ydiff;
            vertices[i].z = 0f;
        }

        line.SetPositions(vertices);

        prevResolution = resolution;
    }
}
