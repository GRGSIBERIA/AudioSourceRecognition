using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Button))]
public class StartButtonScript : MonoBehaviour
{
    // �L���b�V��������R���|�[�l���g
    Button btn;
    Image img;
    Text text;

    // �g�O���{�^���ɂ������̂œs�x������s��
    bool toggle = false;

    // Start is called before the first frame update
    void Start()
    {
        // �R���|�[�l���g�̏���������
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
