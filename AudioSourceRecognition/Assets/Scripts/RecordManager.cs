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
        sound.loop = true;
    }

    public void StartRecording(string deviceName, int bufferingTime, int samplingFrequency)
    {
        this.deviceName = deviceName;
        this.samplingFrequency = samplingFrequency;
        this.bufferingTime = bufferingTime;

        // サンプリングレートが意味不明な場合はエラーメッセージを出す
        if (this.samplingFrequency <= 0)
        {
            Debug.LogError($"Cannot parse sampling frequency. Please over than zero : {this.samplingFrequency}");
            return;
        }

        sound.clip = Microphone.Start(deviceName, true, bufferingTime, samplingFrequency);

        if (Microphone.GetPosition(deviceName) <= 0) { }

        sound.Play();
        Debug.Log("play");
    }

    public void StopRecording()
    {
        sound.Stop();

        Microphone.End(this.deviceName);
    }
}
