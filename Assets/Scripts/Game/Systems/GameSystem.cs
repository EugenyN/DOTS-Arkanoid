using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(GameStateSystemGroup))]
partial struct GameSystem : ISystem, ISystemStartStop
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameSettings>();

        bool vSyncEnabled = PlayerPrefs.GetInt("vSyncEnabled", 1) == 1;
        ApplyGraphicsSettings(vSyncEnabled);
    }
    
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
        
        var inputSettings = SystemAPI.ManagedAPI.GetSingleton<InputSettings>();
        inputSettings.MouseSensitivity = PlayerPrefs.GetFloat("mouseSensitivity", 0.5f);
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
        Time.timeScale = pause ? 0 : 1;
        entityManager.AddSingleFrameComponent(new GamePausedEvent { Paused = pause });
    }

    public static bool IsGamePaused()
    {
        return Time.timeScale == 0;
    }
    
    public static void ApplyGraphicsSettings(bool vSync)
    {
        QualitySettings.vSyncCount = vSync ? 1 : 0;
        
#if UNITY_ANDROID && !UNITY_EDITOR
            // these platforms cap FPS at 30 by default, so we need to unlock it.
            // https://stackoverflow.com/questions/47031279/unity-mobile-device-30fps-locked
            Application.targetFrameRate = (int)System.Math.Round(Screen.currentResolution.refreshRateRatio.value);
#endif
    }
}