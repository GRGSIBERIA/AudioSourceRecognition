using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Button))]
public class StartButtonScript : MonoBehaviour
{
    // キャッシュ化するコンポーネント
    Button btn;
    Image img;
    Text text;

    // トグルボタンにしたいので都度判定を行う
    bool toggle = false;

    // Start is called before the first frame update
    void Start()
    {
        // コンポーネントの初期化処理
        btn = GetComponent<Button>();
        img = GetComponent<Image>();
        text = transform.GetChild(0).GetComponent<Text>();

        btn.onClick.AddListener(() =>
        {
            toggle = !toggle;
            var colors = btn.colors;

            if (toggle)
            {
                img.color = Color.red;
                text.text = "Stop";
            }
            else
            {
                img.color = Color.white;
                text.text = "Start";
            }
        });
    }
}
