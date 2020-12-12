using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColoredCube : MonoBehaviour
{
    [ReadOnly]
    public byte currentColor;
    private MeshRenderer meshRenderer;
    private Color[] colors = new Color[] { Color.white, Color.black, Color.green, Color.yellow, Color.blue, Color.red };
    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        SetRandomColor();
    }

    public void SetRandomColor()
    {
        currentColor = (byte)Random.Range(0, colors.Length);
        meshRenderer.material.color = colors[currentColor];
    }

    public void SetColor(byte index)
    {
        currentColor = index;
        meshRenderer.material.color = colors[index];
    }
}
