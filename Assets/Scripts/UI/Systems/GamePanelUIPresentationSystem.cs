using Unity.Entities;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct GamePanelUIPresentationSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GamePanelUI>();
        state.RequireForUpdate<GameData>();
    }

    public void OnUpdate(ref SystemState state)
    {
        GameUtils.TryGetSingletonEntityManaged<GamePanelUI>(state.EntityManager, out var gamePanelUI);
        var inGameUI = state.EntityManager.GetComponentObject<GamePanelUI>(gamePanelUI);

        var gameData = SystemAPI.GetSingleton<GameData>();
        
        foreach (var command in SystemAPI.Query<ChangeStateCommand>())
        {
            if (command.TargetState == typeof(MainMenuState))
            {
                inGameUI.gameObject.SetActive(false);
            }
            else if (command.TargetState == typeof(GameStartState))
            {
                inGameUI.gameObject.SetActive(true);
                    
                inGameUI.SetPlayersCount(gameData.PlayersCount);
                inGameUI.SetLevel(gameData.Level);
                inGameUI.SetHighScore(gameData.HighScore);
                inGameUI.SetGameAreaMessage($"ROUND {gameData.Level:D2}\n\nREADY!", 3.0f);
            }
            else if (command.TargetState == typeof(GameWinState))
            {
                inGameUI.SetGameAreaMessage(" YOU WIN !");
            }
            else if (command.TargetState == typeof(GameOverState))
            {
                inGameUI.SetGameAreaMessage("GAME OVER");
            }
        }
        
        foreach (var gamePausedEvent in SystemAPI.Query<GamePausedEvent>())
        {
            if (gamePausedEvent.Paused)
                inGameUI.SetGameAreaMessage("PAUSED");
            else
                inGameUI.ClearGameAreaMessages();
        }
        
        foreach (var hiScoreEvent in SystemAPI.Query<HiScoreUpdatedEvent>())
        {
            inGameUI.SetHighScore(hiScoreEvent.Score);
            inGameUI.SetGameAreaMessage($"GAME OVER\n\nNEW HIGH SCORE !\n{hiScoreEvent.Score}");
        }
        
        foreach (var (playerData, playerIndex) in 
                 SystemAPI.Query<PlayerData, PlayerIndex>().WithChangeFilter<PlayerData>())
        {
            inGameUI.SetPlayerLives(playerIndex.Value, playerData.Lives);
            inGameUI.SetPlayerScore(playerIndex.Value, playerData.Score);
                
            if (playerData.Lives == 0)
                inGameUI.SetGameOverMessage(playerIndex.Value, true);
        }
    }
}