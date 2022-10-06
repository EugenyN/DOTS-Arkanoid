using System;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class LevelsSettingsAuthoring : MonoBehaviour
{
    public int BlocksInLine;
    public int BlocksLinesCount;
    public Texture2D[] LevelsData;
    public BlockColorCode[] BlocksPalette;
    
    public class Baker : Baker<LevelsSettingsAuthoring>
    {
        public override void Bake(LevelsSettingsAuthoring authoring)
        {
            AddComponentObject(new LevelsSettings
            {
                BlocksInLine = authoring.BlocksInLine,
                BlockLinesCount = authoring.BlocksLinesCount,
                LevelsData = authoring.LevelsData,
                BlocksPalette = authoring.BlocksPalette
            });
        }
    }
}