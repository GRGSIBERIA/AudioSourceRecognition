using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Button))]
public class StartButtonScript : MonoBehaviour
{
    [SerializeField]
    GameObject recorder;

    [SerializeField]
    GameObject dropdownDevice;

    // キャッシュ化するコンポーネント
    Button btn;
    Image img;
    Text text;
    Dropdown devices;
    RecordManager sound;

    // トグルボタンにしたいので都度判定を行う
    bool toggle = false;

    /// <summary>
    /// レコーディング中か否か
    /// </summary>
    public bool IsRecording { get { return toggle; } }

    // Start is called before the first frame update
    void Start()
    {
        // コンポーネントの初期化処理
        btn = GetComponent<Button>();
        img = GetComponent<Image>();
        text = transform.GetChild(0).GetComponent<Text>();
        sound = recorder.GetComponent<RecordManager>();
        devices = dropdownDevice.GetComponent<Dropdown>();

        btn.onClick.AddListener(() =>
        {
            toggle = !toggle;
            var colors = btn.colors;
            var id = devices.value;

            // 指定されたマイクで完結する
            if (toggle)
            {
                img.color = Color.red;
                text.text = "Stop";
                sound.StartRecording(devices.options[id].text);
            }
            else
            {
                img.color = Color.white;
                text.text = "Start";
                sound.StopRecording();
            }
        });
    }
}
