using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AxisScript : MonoBehaviour
{
    public float Aspect { get; set; }

    LineRenderer line;

    private void Start()
    {
        line = GetComponent<LineRenderer>();
        line.startWidth = 0.02f;
        line.endWidth = 0.02f;

        var pos = new Vector3[]
        {
            new Vector3(-Aspect, 1, 0),
            new Vector3(-Aspect, 0, 0),
            new Vector3(0, 0, 0),
            new Vector3(0, 1, 0)
        };
        line.positionCount = pos.Length;
        line.SetPositions(pos);
    }

    public void OnRenderObject()
    {
        
    }
}
