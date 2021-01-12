using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class RecordManager : MonoBehaviour
{
    AudioSource sound;

    /// <summary>
    /// サンプリング周波数
    /// </summary>
    [SerializeField]
    int samplingFrequency = 44100;

    /// <summary>
    /// 音として記録する時間幅
    /// </summary>
    [SerializeField]
    int bufferingTime = 10;

    /// <summary>
    /// デバイス名
    /// </summary>
    string deviceName;

    // Start is called before the first frame update
    void Start()
    {
        sound = GetComponent<AudioSource>();
        sound.clip = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartRecording(string deviceName)
    {
        this.deviceName = deviceName;

        sound.clip = Microphone.Start(deviceName, true, bufferingTime, samplingFrequency);

        if (Microphone.GetPosition(deviceName) <= 0) { }

        sound.Play();
    }

    public void StopRecording()
    {
        sound.Stop();

        Microphone.End(this.deviceName);
    }
}
