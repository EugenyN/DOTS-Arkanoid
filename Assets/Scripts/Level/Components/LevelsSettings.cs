using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct BlockColorCode
{
    public Color32 Color;
    public BlockTypes Type;
}

public class LevelsSettings : IComponentData
{
    public int BlocksInLine;
    public int BlockLinesCount;
    public Texture2D[] LevelsData;
    public BlockColorCode[] BlocksPalette;
}