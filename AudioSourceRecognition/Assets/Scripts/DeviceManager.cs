using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Dropdown))]
public class DeviceManager : MonoBehaviour
{
    [SerializeField]
    GameObject channelDropdownObject;

    Dropdown drop;

    Dropdown channel;

    /// <summary>
    /// マイクに使えるデバイスリスト
    /// </summary>
    List<Dropdown.OptionData> devices = new List<Dropdown.OptionData>();

    // Start is called before the first frame update
    void Start()
    {
        drop = GetComponent<Dropdown>();
        channel = channelDropdownObject.GetComponent<Dropdown>();

        // オプションにデバイスの名前を登録する
        foreach (var name in Microphone.devices)
        {
            var datum = new Dropdown.OptionData();
            datum.image = null;
            datum.text = name;
            devices.Add(datum);
        }

        drop.options = devices;

        // ドロップダウンが変更されたとき、自動的にチャンネル数を取得する
        drop.onValueChanged.AddListener((int id) =>
        {
            var clip = Microphone.Start(drop.options[id].text, false, 1, 44100);
            var channels = new List<Dropdown.OptionData>();

            // 指定されたデバイスのチャンネル数を登録する
            // 8チャンネルデバイスでも何チャンネルを使用するか指定できる
            // ただし、調べるとUnityのMicrophoneはモノラル録音しか対応していない様子
            for (int i = 0; i < clip.channels; ++i)
            {
                var datum = new Dropdown.OptionData();
                datum.text = (i + 1).ToString();
                datum.image = null;
                channels.Add(datum);
            }

            channel.options = channels;
            Microphone.End(drop.options[id].text);
        });
    }
}
