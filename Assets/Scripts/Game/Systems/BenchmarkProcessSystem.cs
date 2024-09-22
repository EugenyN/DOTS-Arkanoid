using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

[UpdateInGroup(typeof(GameStateSystemGroup))]
public partial struct BenchmarkProcessSystem : ISystem
{
    private const int BallsCount = 1000;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<LevelsSettings>();
        state.RequireForUpdate<GameData>();
        state.RequireForUpdate<StartBenchmarkRequest>();
        state.RequireForUpdate<PaddleData>();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.TempJob);

        var gameData = SystemAPI.GetSingleton<GameData>();
        
        var random = Random.CreateFromIndex(state.GlobalSystemVersion);
        var levelsSettings = SystemAPI.GetSingleton<LevelsSettings>();
        
        foreach (var (ownerPlayerId, paddle) in SystemAPI.Query<RefRO<OwnerPlayerId>>().WithAll<PaddleData>()
                     .WithEntityAccess())
        {
            for (int i = 0; i < BallsCount; i++)
            {
                var center = new float3(
                    levelsSettings.GameAreaWidth / 2.0f,
                    levelsSettings.GameAreaHeight - levelsSettings.BlockLinesCount / 2.0f, 0);
                
                ecb.AddSingleFrameComponent(new BallSpawnRequest
                {
                    Position = center,
                    OwnerPaddle = paddle,
                    OwnerPlayer = ownerPlayerId.ValueRO.Value,
                    StuckToPaddle = false,
                    Velocity = new float3(random.NextFloat2Direction(), 0) * gameData.BallSpeed
                });
            }
        }
        
        var startBenchmarkRequestQuery = SystemAPI.QueryBuilder().WithAll<StartBenchmarkRequest>().Build();
        ecb.DestroyEntity(startBenchmarkRequestQuery, EntityQueryCaptureMode.AtPlayback);
        
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}