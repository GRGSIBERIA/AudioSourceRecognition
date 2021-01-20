using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SpectrumObjectScript : MonoBehaviour
{
    public int SampleN { get; set; }
    public float[] Spectrums { get; set; }
    public float Aspect { get; set; }

    Transform ts;

    // Start is called before the first frame update
    void Start()
    {
        var line = GetComponent<LineRenderer>();
        line.positionCount = SampleN;
        Vector3[] pos = new Vector3[SampleN];

        float xdiff = Aspect * 2f / (float)SampleN;
        float offset = Aspect;

        for (int i = 0; i < SampleN; ++i)
        {
            pos[i] = new Vector3(
                (float)i * xdiff - offset,
                Spectrums[i],
                0f);
        }
        line.SetPositions(pos);

        ts = transform;
    }

    // Update is called once per frame
    void Update()
    {
        ts.localPosition -= Vector3.back * Time.deltaTime;
    }
}
