using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ColorScheme
{
    public string name;
    public Color background;
    public List<Color> blockColor;

    public Color GetRandomColor()
    {
        int i = Random.Range(0, blockColor.Count);
        return blockColor[i];
    }
}
