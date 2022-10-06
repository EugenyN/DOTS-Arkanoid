using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public partial class BlocksSpawnerSystem : SystemBase
{
    private static readonly float3 BlockOffset = new float3(2, -0.5f, 0);
    private static readonly float3 BlockSize = new float3(2, 1f, 1);

    private BeginSimulationEntityCommandBufferSystem _beginSimulationEcbSystem;
    
    [GenerateTestsForBurstCompatibility]
    public struct BlocksSpawnJob : IJobParallelFor
    {
        public Entity BlockPrefab;
        public NativeArray<BlockTypes> LevelData;
        public int BlocksInLine;
        public int BlocksLinesCount;
        public EntityCommandBuffer.ParallelWriter Ecb;

        public void Execute(int index)
        {
            var blockType = LevelData[index];
            if (blockType != BlockTypes.None)
            {
                var position = new float3(index % BlocksInLine, index / BlocksInLine, 0);
                SpawnBlock(index, blockType, position);
            }
        }

        private Entity SpawnBlock(int index,  BlockTypes blockType, float3 position)
        {
            var block = Ecb.Instantiate(index, BlockPrefab);
            Ecb.SetName(index, block, "Block");

            var offset = new float3(0, BlocksLinesCount / 2.0f + 3, 0);
            Ecb.AddComponent(index, block, new Translation { Value = BlockOffset + offset + position * BlockSize });
            Ecb.AddComponent(index, block, new MaterialTextureSTData());

            int frameIndex = blockType switch {
                BlockTypes.Silver => 2 * 6, BlockTypes.Gold => 3 * 6, _ => (int)blockType - 1
            };
            Ecb.AddComponent(index, block, new TextureAnimationData { FrameIndex = frameIndex });
        
            Ecb.AddComponent(index, block, new BlockData { Type = blockType, Health = blockType == BlockTypes.Silver ? 2 : 1 });
                        
            if (blockType == BlockTypes.Gold)
                Ecb.AddComponent<GoldBlock>(index, block);
                        
            Ecb.AddBuffer<SingleFrameComponent>(index, block);

            return block;
        }
    }
    
    protected override void OnCreate()
    {
        base.OnCreate();
        
        _beginSimulationEcbSystem = World.GetExistingSystemManaged<BeginSimulationEntityCommandBufferSystem>();
        
        RequireForUpdate<BlocksSpawnRequest>();
    }
    
    protected override void OnUpdate()
    {
        var levelsSettings = this.GetSingleton<LevelsSettings>();
        
        var gameData = GetSingleton<GameData>();
        int levelsCount = levelsSettings.LevelsData.Length;
        int levelDataIndex = (gameData.Level - 1) % levelsCount;
        
        var levelData = LevelsHelper.GetLevelData(levelsSettings, levelDataIndex);

        var spawnRequest = GetSingleton<BlocksSpawnRequest>();
        
        var ecb = _beginSimulationEcbSystem.CreateCommandBuffer();

        Dependency = new BlocksSpawnJob
        {
            BlockPrefab = spawnRequest.BlockPrefab,
            LevelData = levelData,
            BlocksInLine = levelsSettings.BlocksInLine,
            BlocksLinesCount = levelsSettings.BlockLinesCount,
            Ecb = ecb.AsParallelWriter()
        }.Schedule(levelsSettings.BlockLinesCount * levelsSettings.BlocksInLine, 16, Dependency);
        
        Dependency.Complete();

        levelData.Dispose();
    }
}