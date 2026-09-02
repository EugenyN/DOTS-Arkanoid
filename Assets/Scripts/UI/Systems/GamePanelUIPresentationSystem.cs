using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial class GamePanelUIPresentationSystem : SystemBase
{
    private GamePanelUI _gamePanelUI;

    protected override void OnCreate()
    {
        RequireForUpdate<GameData>();
    }

    protected override void OnUpdate()
    {
        if (_gamePanelUI == null)
            _gamePanelUI = Object.FindAnyObjectByType<GamePanelUI>(FindObjectsInactive.Include);

        var gameData = SystemAPI.GetSingleton<GameData>();

        foreach (var command in SystemAPI.Query<ChangeStateCommand>())
        {
            if (command.TargetState == typeof(MainMenuState))
            {
                _gamePanelUI.gameObject.SetActive(false);
            }
            else if (command.TargetState == typeof(GameStartState))
            {
                _gamePanelUI.gameObject.SetActive(true);

                _gamePanelUI.SetPlayersCount(gameData.PlayersCount);
                _gamePanelUI.SetLevel(gameData.Level);
                _gamePanelUI.SetHighScore(gameData.HighScore);
                _gamePanelUI.SetGameAreaMessage($"ROUND {gameData.Level:D2}\n\nREADY!", 3.0f);
            }
            else if (command.TargetState == typeof(GameWinState))
            {
                _gamePanelUI.SetGameAreaMessage(" YOU WIN !");
            }
            else if (command.TargetState == typeof(GameOverState))
            {
                _gamePanelUI.SetGameAreaMessage("GAME OVER");
            }
        }

        foreach (var gamePausedEvent in SystemAPI.Query<GamePausedEvent>())
        {
            if (gamePausedEvent.Paused)
                _gamePanelUI.SetGameAreaMessage("PAUSED");
            else
                _gamePanelUI.ClearGameAreaMessages();
        }

        foreach (var hiScoreEvent in SystemAPI.Query<HiScoreUpdatedEvent>())
        {
            _gamePanelUI.SetHighScore(hiScoreEvent.Score);
            _gamePanelUI.SetGameAreaMessage($"GAME OVER\n\nNEW HIGH SCORE !\n{hiScoreEvent.Score}");
        }

        foreach (var (playerData, playerIndex) in
                 SystemAPI.Query<PlayerData, PlayerIndex>().WithChangeFilter<PlayerData>())
        {
            _gamePanelUI.SetPlayerLives(playerIndex.Value, playerData.Lives);
            _gamePanelUI.SetPlayerScore(playerIndex.Value, playerData.Score);

            if (playerData.Lives == 0)
                _gamePanelUI.SetGameOverMessage(playerIndex.Value, true);
        }
    }
}