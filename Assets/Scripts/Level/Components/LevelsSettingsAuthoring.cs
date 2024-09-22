using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class LevelsSettingsAuthoring : MonoBehaviour
{
    public int BlocksInLine = 13;
    public int BlocksLinesCount = 18;
    public int GameAreaWidth = 28; // blocks in line * 2 + 2
    public int GameAreaHeight = 30;
    public int MaxPlayers = 4;
    public Texture2D[] LevelsData;
    public BlockColorCode[] BlocksPalette;
    
    public class Baker : Baker<LevelsSettingsAuthoring>
    {
        public override void Bake(LevelsSettingsAuthoring authoring)
        {
            var levelsDataBlob = CreateLevelsDataBlob(authoring);

            AddBlobAsset(ref levelsDataBlob, out var hash);
            
            var entity = GetEntity(TransformUsageFlags.None);
            
            AddComponent(entity, new LevelsSettings
            {
                BlocksInLine = authoring.BlocksInLine,
                BlockLinesCount = authoring.BlocksLinesCount,
                GameAreaWidth = authoring.GameAreaWidth,
                GameAreaHeight = authoring.GameAreaHeight,
                MaxPlayers = authoring.MaxPlayers,
                LevelsDataBlob = levelsDataBlob
            });
        }

        private BlobAssetReference<LevelsData> CreateLevelsDataBlob(LevelsSettingsAuthoring authoring)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            
            ref var levelsData = ref builder.ConstructRoot<LevelsData>();
            
            var blocksPaletteArrayBuilder = builder.Allocate(ref levelsData.BlocksPalette, authoring.BlocksPalette.Length);
            for (int i = 0; i < authoring.BlocksPalette.Length; i++)
                blocksPaletteArrayBuilder[i] = authoring.BlocksPalette[i];
            
            var levelsBlockDataArrayBuilder = builder.Allocate(ref levelsData.LevelsBlockData, authoring.LevelsData.Length);
            for (int i = 0; i < authoring.LevelsData.Length; i++)
            {
                var levelBlocksData = authoring.LevelsData[i].GetRawTextureData<Color32>();
                var levelBlocksArrayBuilder = builder.Allocate(ref levelsBlockDataArrayBuilder[i], levelBlocksData.Length);
                for (int j = 0; j < levelBlocksData.Length; j++)
                    levelBlocksArrayBuilder[j] = levelBlocksData[j];
            }
            
            var result = builder.CreateBlobAssetReference<LevelsData>(Allocator.Persistent);
            
            builder.Dispose();
            
            return result;
        }
    }
}