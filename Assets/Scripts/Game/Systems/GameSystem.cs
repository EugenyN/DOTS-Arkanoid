using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(GameStateSystemGroup))]
partial struct GameSystem : ISystem, ISystemStartStop
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameSettings>();
    }
    
    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        var gameSettings = SystemAPI.GetSingleton<GameSettings>();
        
        var gameData = state.EntityManager.CreateEntity();
        state.EntityManager.SetName(gameData, "GameData");
        
        state.EntityManager.AddComponentData(gameData, new GameData
        {
            Level = gameSettings.StartLevel, HighScore = gameSettings.HighScore
        });
        state.EntityManager.AddComponentData(gameData, new GameStateData
        {
            CurrentState = ComponentType.ReadWrite<MainMenuState>()
        });
    }

    public void OnStopRunning(ref SystemState state)
    {
    }

    public static void StartGame(EntityManager entityManager, int playersCount)
    {
        GameUtils.TryGetSingleton<GameSettings>(entityManager, out var gameSettings);
        GameUtils.TryGetSingletonRW<GameData>(entityManager, out var gameData);
        
        gameData.ValueRW.Level = gameSettings.StartLevel;
        gameData.ValueRW.PlayersCount = playersCount;

        entityManager.AddSingleFrameComponent(ChangeStateCommand.Create<GameStartState>());
    }

    public static void ExitGame(EntityManager entityManager)
    {
        var playersDataQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<PlayerData>().Build(entityManager);
        entityManager.AddSingleFrameComponent(new LevelDespawnRequest());
        entityManager.DestroyEntity(playersDataQuery);
        entityManager.AddSingleFrameComponent(ChangeStateCommand.Create<MainMenuState>());
        
        SetPause(entityManager, false);
        
        AudioSystem.StopAudio(entityManager);
    }

    public static void SetPause(EntityManager entityManager, bool pause)
    {
        UnityEngine.Time.timeScale = pause ? 0 : 1;
        entityManager.AddSingleFrameComponent(new GamePausedEvent { Paused = pause });
    }

    public static bool IsGamePaused()
    {
        return UnityEngine.Time.timeScale == 0;
    }
}