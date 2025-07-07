using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct PlayerPanelUI
{
    [SerializeField] public GameObject PanelObject;
    [SerializeField] public Text ScoreText;
    [SerializeField] public Image[] LiveImages;
    [SerializeField] public Text GameOverText;
}

public class GamePanelUI : MonoBehaviour
{
    [SerializeField] private Text _highScoreText;
    [SerializeField] private Text _roundText;
    [SerializeField] private Text _messageText;
    [SerializeField] private PlayerPanelUI[] _playerPanelUis;
    [SerializeField] private Text _roundLabelText;
    [SerializeField] private Text _creditsLabelText;
    
    private float? _messageTime;
    private readonly HashSet<int> _gameOverPlayers = new HashSet<int>();

    private void OnEnable()
    {
        if (World.DefaultGameObjectInjectionWorld == null)
            return;

        _messageTime = null;

        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        GameUtils.TryGetSingleton<LevelsSettings>(entityManager, out var levelsSettings);
        
        for (int i = 0; i < levelsSettings.MaxPlayers; i++)
        {
            SetGameOverMessage(i, false);
            SetPlayerScore(i, 0);
            SetPlayerLives(i, 3);
        }
    }
    
    public void OnExitButtonClick()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        GameSystem.ExitGame(entityManager);
    }

    public void OnPauseButtonClick()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        GameSystem.SetPause(entityManager, !GameSystem.IsGamePaused());
    }

    public void OnFireButtonClick()
    {
        if (!GameSystem.IsGamePaused())
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            PaddleInputPollSystem.DoInputAction(entityManager, 0, InputActionType.Fire);
        }
    }

    public void SetPlayersCount(int playersCount)
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        GameUtils.TryGetSingleton<LevelsSettings>(entityManager, out var levelsSettings);

        for (int i = 0; i < levelsSettings.MaxPlayers; i++)
        {
            _playerPanelUis[i].PanelObject.SetActive(i < playersCount);
            _playerPanelUis[i].GameOverText.gameObject.SetActive(false);
        }

        if (playersCount > 2)
        {
            _creditsLabelText.gameObject.SetActive(false);
            _roundLabelText.gameObject.SetActive(false);
        }
    }
    
    public void SetLevel(int level)
    {
        _roundText.text = level.ToString();
    }
    
    public void SetHighScore(int highScore)
    {
        _highScoreText.text = highScore.ToString();
    }
    
    public void SetPlayerLives(int playerIndex, int lives)
    {
        var playerPanelUI = _playerPanelUis[playerIndex];
        for (int i = 0; i < 6; i++)
            playerPanelUI.LiveImages[i].gameObject.SetActive(lives > i);
    }
    
    public void SetPlayerScore(int playerIndex, int score)
    {
        var playerPanelUI = _playerPanelUis[playerIndex];
        playerPanelUI.ScoreText.text = score.ToString();
    }
    
    public void SetGameOverMessage(int playerIndex, bool gameOver)
    {
        if (gameOver)
            _gameOverPlayers.Add(playerIndex);
        else
            _gameOverPlayers.Remove(playerIndex);
    }
    
    public void SetGameAreaMessage(string message, float? time = null)
    {
        _messageText.text = message;
        _messageTime = time;
    }

    public void ClearGameAreaMessages()
    {
        _messageText.text = string.Empty;
        _messageTime = null;
    }
    
    private void Update()
    {
        if (_messageTime.HasValue)
        {
            _messageTime -= Time.deltaTime;
            if (_messageTime <= 0)
            {
                _messageTime = null;
                ClearGameAreaMessages();
            }
        }
        
        foreach (var playerIndex in _gameOverPlayers)
            _playerPanelUis[playerIndex].GameOverText.gameObject.SetActive(Time.time % 2 > 1);
    }
}