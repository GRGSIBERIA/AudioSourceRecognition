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
    float aspect = 6;

    RecordManager recorder;

    [SerializeField]
    int startSample = 65536;

    [SerializeField]
    int fourierCount = 1;

    BlackmanHarrisWindow windows;

    FourierTransform fft;

    NativeArray<float> fourier;

    List<Vector3[]> vertices = new List<Vector3[]>();

    LineRenderer line;

    Vector3[] TemplateVector { get; set; }


    void InitializeWindow()
    {
        windows = new BlackmanHarrisWindow(startSample, recorder.SamplingRate);
        fft = new FourierTransform(recorder.SamplingRate, windows);
        vertices = new List<Vector3[]>();

        float offset = aspect;
        float xdiff = aspect * 2f / (float)startSample;

        TemplateVector = new Vector3[startSample];
        for (int i = 0; i < TemplateVector.Length; ++i)
        {
            TemplateVector[i] = new Vector3(i * xdiff - offset, 0f, 0f);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        recorder = recordObject.GetComponent<RecordManager>();
        line = GetComponent<LineRenderer>();
        line.startWidth = 0.01f;
        line.endWidth = 0.01f;
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

        var buffer = recorder.GetData();

        // フーリエのバッファに蓄えさせる
        fourier = fft.FFT(buffer);

        // フーリエ解析するごとに後ろに下がる
        var local = transform.localPosition;
        local.z -= Time.deltaTime;
        transform.localPosition = local;
    }

    public void OnRenderObject()
    {
        if (!recorder.IsRecording) return;

        float offset = aspect;
        float xdiff = aspect * 2f / (float)startSample;

        Vector3[] previours = new Vector3[line.positionCount + startSample];
        line.GetPositions(previours);

        for (int i = 0; i < fourier.Length; ++i)
        {
            previours[line.positionCount + i] = new Vector3(
                i * xdiff - offset,
                fourier[i],
                Time.realtimeSinceStartup);
        }

        line.positionCount = previours.Length;
        line.SetPositions(previours);
    }

    private void OnDestroy()
    {
        windows.Dispose();
        fft.Dispose();
    }
}


