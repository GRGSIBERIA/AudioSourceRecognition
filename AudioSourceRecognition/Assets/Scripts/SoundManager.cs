using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    AudioSource sound;

    /// <summary>
    /// 再生デバイスのリスト
    /// </summary>
    [SerializeField]
    List<string> devices;

    /// <summary>
    /// devicesの中で利用したいデバイスのID
    /// </summary>
    [SerializeField]
    int usingDeviceId = 0;

    /// <summary>
    /// サンプリング周波数
    /// </summary>
    [SerializeField]
    int samplingFrequency = 44100;

    /// <summary>
    /// デバイス名
    /// </summary>
    string deviceName;

    // Start is called before the first frame update
    void Start()
    {
        sound = GetComponent<AudioSource>();

        devices = new List<string>(Microphone.devices);

        deviceName = devices[usingDeviceId];

        if (devices.Count > 0)
        {
            // 指定したサンプリング周波数で10秒だけバッファを設ける
            // ループ録音することでクリップ終端に達してもクリップ先端から録音を続ける
            sound.clip = Microphone.Start(deviceName, true, 10, samplingFrequency);

            // デバイスが再生されるまで待機
            if (Microphone.GetPosition(deviceName) <= 0) { }

            sound.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}
