using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;

[UpdateInGroup(typeof(GameStateSystemGroup))]
public partial struct GameWinSystem : ISystem, ISystemStartStop
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameStateData>();
        state.RequireForUpdate<GameWinState>();
    }
    
    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        var gameState = SystemAPI.GetSingleton<GameStateData>();
        gameState.StateTimer = 3.0f;
        SystemAPI.SetSingleton(gameState);
        
        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        
        foreach (var (_, entity) in SystemAPI.Query<PaddleData>().WithEntityAccess())
        {
            ecb.RemoveComponent<PhysicsCollider>(entity);
            ecb.RemoveComponent<PaddleInputData>(entity);
        }
        
        foreach (var (_, entity) in SystemAPI.Query<PhysicsVelocity>().WithEntityAccess())
            ecb.RemoveComponent<PhysicsVelocity>(entity);
        
        foreach (var (_, entity) in SystemAPI.Query<PlayTextureAnimation>().WithEntityAccess())
            ecb.RemoveComponent<PlayTextureAnimation>(entity);
            
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
        
        AudioSystem.PlayAudio(state.EntityManager, AudioClipKeys.RoundCleared);
    }

    public void OnStopRunning(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        
        foreach (var (gameData, gameStateData) in SystemAPI.Query<RefRW<GameData>, RefRW<GameStateData>>())
        {
            if (gameStateData.ValueRW.StateTimer > 0)
                gameStateData.ValueRW.StateTimer -= SystemAPI.Time.DeltaTime;
            else
            {
                gameData.ValueRW.Level++;
            
                ecb.AddSingleFrameComponent(new LevelDespawnRequest());
                ecb.AddSingleFrameComponent(ChangeStateCommand.Create<GameStartState>());
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}