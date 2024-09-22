using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(GameStateSystemGroup))]
public partial struct GameOverSystem : ISystem, ISystemStartStop
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameStateData>();
        state.RequireForUpdate<GameData>();
        state.RequireForUpdate<GameOverState>();
    }
    
    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        int highScore = 0;
        foreach (var playerData in SystemAPI.Query<RefRO<PlayerData>>()) 
            highScore = math.max(highScore, playerData.ValueRO.Score);
        
        var gameData = SystemAPI.GetSingleton<GameData>();
        if (highScore > gameData.HighScore)
        {
            gameData.HighScore = highScore;
            state.EntityManager.AddSingleFrameComponent(new HiScoreUpdatedEvent { Score = highScore });
        }
        SystemAPI.SetSingleton(gameData);
        
        var gameState = SystemAPI.GetSingleton<GameStateData>();
        gameState.StateTimer = 5.0f;
        SystemAPI.SetSingleton(gameState);

        AudioSystem.PlayAudio(state.EntityManager, AudioClipKeys.GameOver);
    }
    
    public void OnStopRunning(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var gameState = SystemAPI.GetSingleton<GameStateData>();

        if (gameState.StateTimer > 0)
        {
            gameState.StateTimer -= SystemAPI.Time.DeltaTime;
            SystemAPI.SetSingleton(gameState);
        }
        else
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            ecb.AddSingleFrameComponent(new LevelDespawnRequest());
            ecb.DestroyEntity(SystemAPI.QueryBuilder().WithAll<PlayerData>().Build(), EntityQueryCaptureMode.AtPlayback);
            ecb.AddSingleFrameComponent(ChangeStateCommand.Create<MainMenuState>());
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}