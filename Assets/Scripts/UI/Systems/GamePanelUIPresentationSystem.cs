using System;
using Unity.Entities;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial class GamePanelUIPresentationSystem : SystemBase
{
    private EntityQuery _gamePanelUIQuery;
    private GamePanelUI _inGameUI;
    
    protected override void OnCreate()
    {
        base.OnCreate();
        
        _gamePanelUIQuery = GetEntityQuery(typeof(GamePanelUI));
        
        RequireForUpdate<GamePanelUI>();
    }

    protected override void OnUpdate()
    {
        if (_inGameUI == null)
            _inGameUI = EntityManager.GetComponentObject<GamePanelUI>(_gamePanelUIQuery.GetSingletonEntity());
        
        Entities
            .WithoutBurst()
            .ForEach((in ChangeStateCommand command) =>
            {
                if (command.TargetState == typeof(MainMenuState))
                {
                    _inGameUI.gameObject.SetActive(false);
                }
                else if (command.TargetState == typeof(GameProcessState))
                {
                    _inGameUI.gameObject.SetActive(true);
                        
                    var gameData = GetSingleton<GameData>();
                    _inGameUI.SetPlayersCount(gameData.PlayersCount);
                    _inGameUI.SetLevel(gameData.Level);
                    _inGameUI.SetHighScore(gameData.HighScore);
                    _inGameUI.SetGameAreaMessage($"ROUND {gameData.Level:D2}\n\nREADY!", 3.0f);
                }
                else if (command.TargetState == typeof(GameWinState))
                {
                    _inGameUI.SetGameAreaMessage(" YOU WIN !");
                }
                else if (command.TargetState == typeof(GameOverState))
                {
                    _inGameUI.SetGameAreaMessage("GAME OVER");
                }
                
            }).Run();

        Entities.WithoutBurst().ForEach((in GamePausedEvent gamePausedEvent) =>
        {
            if (gamePausedEvent.Paused)
                _inGameUI.SetGameAreaMessage("PAUSED");
            else
                _inGameUI.ClearGameAreaMessages();
        }).Run();

        Entities.WithoutBurst().ForEach((in HiScoreUpdatedEvent hiScoreEvent) =>
        {
            _inGameUI.SetHighScore(hiScoreEvent.Score);
            _inGameUI.SetGameAreaMessage($"GAME OVER\n\nNEW HIGH SCORE !\n{hiScoreEvent.Score}");
        }).Run();
        
        Entities
            .WithoutBurst()
            .WithChangeFilter<PlayerData>()
            .ForEach((Entity entity, in PlayerData playerData, in PlayerIndex playerIndex) =>
            {
                _inGameUI.SetPlayerLives(playerIndex.Value, playerData.Lives);
                _inGameUI.SetPlayerScore(playerIndex.Value, playerData.Score);
                
                if (playerData.Lives == 0)
                    _inGameUI.SetGameOverMessage(playerIndex.Value, true);
            }).Run();
    }
}