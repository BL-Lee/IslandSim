using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class ColourData
{
    public Material mat;
    public Color[] baseColours;
    [Range(0,1)]
    public float[] baseStartHeights;
    [Range(0,1)]
    public float[] baseBlends;
}
