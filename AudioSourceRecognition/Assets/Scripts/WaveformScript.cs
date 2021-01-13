using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveformScript : MonoBehaviour
{
    Material line;

    [SerializeField]
    GameObject recordManager;

    RecordManager recorder;

    // Start is called before the first frame update
    void Start()
    {
        recorder = recordManager.GetComponent<RecordManager>();

        Shader shader = Shader.Find("Hidden/Internal-Colored");
        line = new Material(shader);
        line.hideFlags = HideFlags.HideAndDontSave;
        line.SetInt("_srcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        line.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        line.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        line.SetInt("_ZWrite", 0);
    }

    // Update is called once per frame
    public void OnRenderObject()
    {
        var waveform = recorder.GetData();

        if (line == null)
        {
            Debug.LogError("Forgets to assign color of material.");
            return;
        }

        line.SetPass(0);

        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);

        var resolution = Screen.currentResolution;
        float xdiff = (float)resolution.width / waveform.Length;
        float ydiff = Mathf.Pow(2, 24) / (float)resolution.height;

        GL.Begin(GL.LINE_STRIP);
        for (int i = 0; i < waveform.Length; ++i)
        {
            GL.Color(Color.red);

            Vector3 pos = new Vector3(i * xdiff, waveform[i] * ydiff, 0);
            GL.Vertex(pos);
        }
        GL.End();
        GL.PopMatrix();
    }
}
