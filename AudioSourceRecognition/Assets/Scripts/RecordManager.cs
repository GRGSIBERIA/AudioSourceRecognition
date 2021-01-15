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
    /// バッファリング秒数とサンプリングレートに対するサンプル数
    /// </summary>
    public int NumofSamples { get { return samplingFrequency * bufferingTime; } }

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
        sound.mute = true;
    }

    /// <summary>
    /// StartRecording時に初期化
    /// リングバッファ
    /// </summary>
    float[] buffer;

    /// <summary>
    /// 整列済みのバッファ
    /// </summary>
    float[] ordered;

    public float[] GetData()
    {
        // 整列済みのバッファを返す
        return ordered;
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
        buffer = new float[bufferingTime * samplingFrequency];
        ordered = new float[buffer.Length];

        if (Microphone.GetPosition(deviceName) <= 0) { }

        sound.Play();
        Debug.Log("play");
    }

    private void Update()
    {
        // アップデートごとにマイクから取得した音源をコピーする
        int position = Microphone.GetPosition(deviceName);  // マイク端の位置だと思う

        sound.clip.GetData(buffer, 0);

        // リングバッファ端の前から端までのコピー
        System.Array.Copy(buffer, position, ordered, 0, buffer.Length - position);

        // リングバッファの最初からマイク位置までのコピー
        System.Array.Copy(buffer, 0, ordered, buffer.Length - position, position);
    }

    /// <summary>
    /// レコーディング中かどうか返す
    /// </summary>
    public bool IsRecording { get { return Microphone.IsRecording(deviceName); } }

    public void StopRecording()
    {
        sound.Stop();

        Microphone.End(this.deviceName);
    }
}
