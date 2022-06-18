using Unity.Entities;
using UnityEngine;

// use custom Authoring class to avoid RegisterBinding warning. May be fixed by Unity in future.
[DisallowMultipleComponent]
public class LevelsSettingsAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public int BlocksInLine;
    public int BlocksLinesCount;
    public Texture2D[] LevelsData;
    public BlockColorCode[] BlocksPalette;
    
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var component = new LevelsSettings
        {
            BlocksInLine = BlocksInLine,
            BlockLinesCount = BlocksLinesCount,
            LevelsData = LevelsData,
            BlocksPalette = BlocksPalette
        };
        dstManager.AddComponentData(entity, component);
    }
}