using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(GameStateSystemGroup))]
public partial struct GameStartSystem : ISystem, ISystemStartStop
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameStateData>();
        state.RequireForUpdate<GameSettings>();
        state.RequireForUpdate<GameData>();
        state.RequireForUpdate<LevelsSettings>();
        state.RequireForUpdate<ScenePrefabs>();
        state.RequireForUpdate<GameStartState>();
    }
	
    public void OnStartRunning(ref SystemState state)
    {
        var prefabs = SystemAPI.GetSingleton<ScenePrefabs>();
	    
        var levelsSettings = SystemAPI.GetSingleton<LevelsSettings>();
	    
        var cameraPos = new float3(levelsSettings.GameAreaWidth / 2.0f, levelsSettings.GameAreaHeight / 2.0f, -5);
        var camera = Object.FindFirstObjectByType<SceneCamera>();
        camera.Setup(cameraPos, levelsSettings.GameAreaHeight / 2.0f);

        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        
        ecb.Instantiate(prefabs.LevelEntityPrefab);
	    
        var playerDataQuery = SystemAPI.QueryBuilder().WithAll<PlayerData>().Build();
        if (playerDataQuery.IsEmpty)
        {
            var gameData = SystemAPI.GetSingleton<GameData>();
            for (int i = 0; i < gameData.PlayersCount; i++)
                CreatePlayer(ecb, i);
        }
        
        ecb.AddSingleFrameComponent(new BlocksSpawnRequest { BlockPrefab = prefabs.BlockEntityPrefab });
        
        AudioSystem.PlayAudio(ecb, AudioClipKeys.RoundStart);
	    
        if (SystemAPI.HasSingleton<BenchmarkModeTag>())
            ecb.AddComponent(ecb.CreateEntity(), new StartBenchmarkRequest());
        
        var gameState = SystemAPI.GetSingleton<GameStateData>();
        gameState.StateTimer = 2.0f;
        SystemAPI.SetSingleton(gameState);
	    
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
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
            
            foreach (var (playerData, player) in SystemAPI.Query<RefRO<PlayerData>>().WithEntityAccess())
            {
                if (playerData.ValueRO.Lives != 0)
                    ecb.AddSingleFrameComponent(new PaddleSpawnRequest { OwnerPlayer = player });
            }
			
            var levelsSettings = SystemAPI.GetSingleton<LevelsSettings>();
            SetBallSpeedForLevel(levelsSettings);
            
            ecb.AddSingleFrameComponent(ChangeStateCommand.Create<GameProcessState>());
			
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
	
    private Entity CreatePlayer(EntityCommandBuffer ecb, int playerIndex)
    {
        var gameSettings = SystemAPI.GetSingleton<GameSettings>();
        var entity = ecb.CreateEntity();
        ecb.SetName(entity, "Player" + playerIndex);

        ecb.AddComponent(entity, new PlayerData { Lives = gameSettings.PlayerStartLives, Score = 0 });
        ecb.AddComponent(entity, new PlayerIndex { Value = playerIndex});
        ecb.AddBuffer<SingleFrameComponent>(entity);
		
        return entity;
    }
	
    private void SetBallSpeedForLevel(LevelsSettings levelsSettings)
    {
        var gameData = SystemAPI.GetSingleton<GameData>();
        var gameSettings = SystemAPI.GetSingleton<GameSettings>();
        
        int levelsCount = levelsSettings.LevelsDataBlob.Value.LevelsBlockData.Length;
        int levelDifficulty = gameData.Level / (levelsCount + 1);
        gameData.BallSpeed = gameSettings.BallSpeed + 1.2f * levelDifficulty;
        
        SystemAPI.SetSingleton(gameData);
    }
}