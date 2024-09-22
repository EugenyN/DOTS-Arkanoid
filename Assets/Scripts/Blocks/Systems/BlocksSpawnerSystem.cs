using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup), OrderFirst = true)]
public partial struct BlocksSpawnerSystem : ISystem
{
    private static readonly float3 BlockOffset = new float3(2, -0.5f, 0);
    private static readonly float3 BlockSize = new float3(2, 1f, 1);

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<GameData>();
        state.RequireForUpdate<LevelsSettings>();
        state.RequireForUpdate<BlocksSpawnRequest>();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var levelsSettings = SystemAPI.GetSingleton<LevelsSettings>();
        
        var gameData = SystemAPI.GetSingleton<GameData>();
        int levelsCount = levelsSettings.LevelsDataBlob.Value.LevelsBlockData.Length;
        int levelDataIndex = (gameData.Level - 1) % levelsCount;
        
        var levelData = GetLevelData(levelsSettings, levelDataIndex);

        var spawnRequest = SystemAPI.GetSingleton<BlocksSpawnRequest>();
        
        var ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();

        state.Dependency = new BlocksSpawnJob
        {
            BlockPrefab = spawnRequest.BlockPrefab,
            LevelData = levelData,
            BlocksInLine = levelsSettings.BlocksInLine,
            BlocksLinesCount = levelsSettings.BlockLinesCount,
            GameAreaHeight = levelsSettings.GameAreaHeight,
            Ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
        }.Schedule(levelsSettings.BlockLinesCount * levelsSettings.BlocksInLine, 16, state.Dependency);
        
        levelData.Dispose(state.Dependency);
    }
    
    private static NativeArray<BlockTypes> GetLevelData(LevelsSettings levelsSettings, int levelIndex)
    {
        ref var levelsData = ref levelsSettings.LevelsDataBlob.Value;
        
        if (levelsData.LevelsBlockData.Length <= levelIndex)
            throw new ArgumentException($"Level data for level #{levelIndex + 1} not found !");

        ref var levelBlockData = ref levelsData.LevelsBlockData[levelIndex];
        ref var blocksPaletteRef = ref levelsData.BlocksPalette;
        
        var blocksPalette = new NativeHashMap<Color, BlockTypes>(blocksPaletteRef.Length, Allocator.Temp);
        for (int i = 0; i < blocksPaletteRef.Length; i++)
            blocksPalette.Add(blocksPaletteRef[i].Color, blocksPaletteRef[i].Type);
        
        var levelData = new NativeArray<BlockTypes>(levelBlockData.Length, Allocator.TempJob);

        for (int i = 0; i < levelBlockData.Length; i++)
            levelData[i] = blocksPalette[levelBlockData[i]];
        
        return levelData;
    }
    
    [BurstCompile]
    private struct BlocksSpawnJob : IJobParallelFor
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        public Entity BlockPrefab;
        [ReadOnly] public NativeArray<BlockTypes> LevelData;
        public int BlocksInLine;
        public int BlocksLinesCount;
        public int GameAreaHeight;

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

            var offset = new float3(0, GameAreaHeight - BlocksLinesCount, 0);
            Ecb.AddComponent(index, block, LocalTransform.FromPosition(BlockOffset + offset + position * BlockSize));
            Ecb.AddComponent(index, block, new MaterialTextureSTData());

            int frameIndex = blockType switch {
                BlockTypes.Silver => 2 * 6, BlockTypes.Gold => 3 * 6, _ => (int)blockType - 1
            };
            Ecb.AddComponent(index, block, new TextureAnimationData { FrameIndex = frameIndex });
        
            Ecb.AddComponent(index, block, new BlockData { Type = blockType, Health = blockType == BlockTypes.Silver ? 2 : 1 });
                        
            if (blockType == BlockTypes.Gold)
                Ecb.AddComponent<GoldBlock>(index, block);
            
            Ecb.AddComponent(index, block, new HitByBallEvent());
            Ecb.SetComponentEnabled<HitByBallEvent>(index, block, false);
            
            Ecb.AddComponent(index, block, new HitByLaserEvent());
            Ecb.SetComponentEnabled<HitByLaserEvent>(index, block, false);
            
            return block;
        }
    }
}