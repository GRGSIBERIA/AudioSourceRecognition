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


    void InitializeWindow()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
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

    }

    public void OnRenderObject()
    {
        if (!recorder.IsRecording) return;

    }

}


