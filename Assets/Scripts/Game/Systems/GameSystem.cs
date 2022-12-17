using Unity.Entities;

[UpdateInGroup(typeof(InitializationSystemGroup))]
partial class GameSystem : SystemBase
{
    private EntityQuery _gameDataQuery;
    private EntityQuery _playersDataQuery;
    
    protected override void OnCreate()
    {
        base.OnCreate();
        
        _gameDataQuery = EntityManager.CreateEntityQuery(ComponentType.ReadWrite<GameData>());
        _playersDataQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<PlayerData>());
        
        RequireForUpdate<GameSettings>();
    }

    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        
        var gameSettings = SystemAPI.GetSingleton<GameSettings>();

        var gameData = EntityManager.CreateEntity(typeof(GameData), typeof(GameStateData));
        EntityManager.SetName(gameData, "GameData");
        
        SystemAPI.SetComponent(gameData, new GameData { Level = gameSettings.StartLevel, HighScore = gameSettings.HighScore });
        SystemAPI.SetComponent(gameData, new GameStateData { CurrentState = typeof(MainMenuState) });
    }

    protected override void OnUpdate()
    {
    }
    
    public void StartGame(int playersCount)
    {
        var gameSettingsQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<GameSettings>());
        var gameSettings = gameSettingsQuery.GetSingleton<GameSettings>();
        
        var gameData = _gameDataQuery.GetSingleton<GameData>();
        gameData.Level = gameSettings.StartLevel;
        gameData.PlayersCount = playersCount;
        _gameDataQuery.SetSingleton(gameData);

        EntityManager.AddSingleFrameComponent(new ChangeStateCommand { TargetState = typeof(GameProcessState) });
    }

    public void ExitGame()
    {
        EntityManager.AddSingleFrameComponent(new LevelDespawnRequest());
        EntityManager.DestroyEntity(_playersDataQuery);
        EntityManager.AddSingleFrameComponent(new ChangeStateCommand { TargetState = typeof(MainMenuState) });
        
        SetPause(false);
        
        AudioSystem.StopAudio(EntityManager);
    }

    public void SetPause(bool pause)
    {
        UnityEngine.Time.timeScale = pause ? 0 : 1;
        EntityManager.AddSingleFrameComponent(new GamePausedEvent { Paused = pause });
    }

    public bool IsGamePaused()
    {
        return UnityEngine.Time.timeScale == 0;
    }
}