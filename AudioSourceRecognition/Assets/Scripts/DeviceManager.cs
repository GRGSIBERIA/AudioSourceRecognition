using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Dropdown))]
public class DeviceManager : MonoBehaviour
{
    [SerializeField]
    GameObject errorMessage;

    Dropdown drop;

    /// <summary>
    /// マイクに使えるデバイスリスト
    /// </summary>
    List<Dropdown.OptionData> devices = new List<Dropdown.OptionData>();

    // Start is called before the first frame update
    void Start()
    {
        drop = GetComponent<Dropdown>();

        // オプションにデバイスの名前を登録する
        foreach (var name in Microphone.devices)
        {
            var datum = new Dropdown.OptionData();
            datum.image = null;
            datum.text = name;
            devices.Add(datum);
        }

        drop.options = devices;
    }
}
