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

    [SerializeField]
    GameObject samplingField;

    [SerializeField]
    GameObject bufferingField;

    [SerializeField]
    GameObject analizerObject;

    [SerializeField]
    GameObject errorText;

    // キャッシュ化するコンポーネント
    Button btn;
    Image img;
    Text text;
    Dropdown devices;
    RecordManager sound;
    InputField samplingFrequency;
    InputField bufferingTime;
    Text errorMessage;
    AnalyzerScript analyzer;

    // トグルボタンにしたいので都度判定を行う
    bool toggle = false;

    /// <summary>
    /// レコーディング中か否か
    /// </summary>
    public bool IsRecording { get { return toggle; } }

    void ShowToStop()
    {
        img.color = Color.red;
        text.text = "Stop";
        toggle = true;
    }

    void ShowToStart()
    {
        img.color = Color.white;
        text.text = "Start";
        toggle = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        // コンポーネントの初期化処理
        btn = GetComponent<Button>();
        img = GetComponent<Image>();
        text = transform.GetChild(0).GetComponent<Text>();
        sound = recorder.GetComponent<RecordManager>();
        devices = dropdownDevice.GetComponent<Dropdown>();
        samplingFrequency = samplingField.GetComponent<InputField>();
        bufferingTime = bufferingField.GetComponent<InputField>();
        errorMessage = errorText.GetComponent<Text>();
        analyzer = analizerObject.GetComponent<AnalyzerScript>();

        btn.onClick.AddListener(() =>
        {
            // サウンドデバイスを持っていない場合は、エラーメッセージを出して処理を止める
            if (devices.options == null || devices.options.Count <= 0)
            {
                errorMessage.text = "This computer has not Microphone devices.";
                return;
            }

            var colors = btn.colors;
            var id = devices.value;

            // 指定されたマイクで完結する
            if (!IsRecording)
            {
                // ボタンがクリックされたから止めるボタンを表示
                ShowToStop();
                try
                {
                    int freq = int.Parse(samplingFrequency.text);
                    int time = int.Parse(bufferingTime.text);
                    sound.StartRecording(devices.options[id].text, time, freq);
                    analyzer.InvokeAnalyze();
                    errorMessage.text = "";     // 何事もなく最後まで実行できた
                }
                catch (System.Exception e)
                {
                    // intのParseに失敗した場合が大半だと思う
                    // 強制的にStartボタンを表示させる
                    ShowToStart();
                    errorMessage.text = e.Message;
                    Debug.LogError(e.Message);
                }
            }
            else
            {
                ShowToStart();
                sound.StopRecording();
            }
        });
    }
}
