using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct BlockColorCode
{
    public Color32 Color;
    public BlockTypes Type;
}

[Serializable]
public struct LevelsData
{
    public BlobArray<BlobArray<Color32>> LevelsBlockData;
    public BlobArray<BlockColorCode> BlocksPalette;
}

public struct LevelsSettings : IComponentData
{
    public int BlocksInLine;
    public int BlockLinesCount;
    public int GameAreaWidth;
    public int GameAreaHeight;
    public int MaxPlayers;
    public BlobAssetReference<LevelsData> LevelsDataBlob;
}