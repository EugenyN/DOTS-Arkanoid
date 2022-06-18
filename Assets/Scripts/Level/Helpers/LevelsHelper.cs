using System;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public static class LevelsHelper
{
    public static NativeArray<BlockTypes> GetLevelData(LevelsSettings levelsSettings, int levelIndex)
    {
        if (levelsSettings.LevelsData.Length <= levelIndex)
            throw new ArgumentException($"Level data for level #{levelIndex + 1} not found !");
        
        var levelColorData = levelsSettings.LevelsData[levelIndex].GetRawTextureData<Color32>();
        var blocksPalette = levelsSettings.BlocksPalette.ToDictionary(c => c.Color, t => t.Type);
        var levelData = new NativeArray<BlockTypes>(levelColorData.Length, Allocator.TempJob);
        
        for (int i = 0; i < levelColorData.Length; i++)
            levelData[i] = blocksPalette[levelColorData[i]];
        
        levelColorData.Dispose();
        
        return levelData;
    }
}