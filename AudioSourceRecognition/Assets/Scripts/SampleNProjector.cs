using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class SampleNProjector : MonoBehaviour
{
    [SerializeField]
    GameObject textObject;

    InputField input;
    Text text;

    // Start is called before the first frame update
    void Start()
    {
        text = textObject.GetComponent<Text>();
        input = GetComponent<InputField>();

        input.onEndEdit.AddListener((string str) =>
        {
            text.text = $"= {1 << int.Parse(str)}";
        });
    }
}
