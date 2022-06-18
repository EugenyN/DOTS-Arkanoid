using System;
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

    private GameSystem _gameSystem;
    private PaddleInputPollSystem _paddleInputPollSystem;

    private float? _messageTime;
    private readonly bool[] _gameOver = new bool[GameConst.MaxPlayers];

    private void OnEnable()
    {
        _gameSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<GameSystem>();
        _paddleInputPollSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<PaddleInputPollSystem>();

        _messageTime = null;

        for (int i = 0; i < GameConst.MaxPlayers; i++)
        {
            SetGameOverMessage(i, false);
            SetPlayerScore(i, 0);
            SetPlayerLives(i, 3);
        }
    }
    
    public void OnExitButtonClick()
    {
        _gameSystem.ExitGame();
    }

    public void OnPauseButtonClick()
    {
        _gameSystem.SetPause(!_gameSystem.IsGamePaused());
    }

    public void OnFireButtonClick()
    {
        _paddleInputPollSystem.DoInputAction(0, InputActionType.Fire);
    }

    public void SetPlayersCount(int playersCount)
    {
        for (int i = 0; i < GameConst.MaxPlayers; i++)
            _playerPanelUis[i].PanelObject.SetActive(i < playersCount);

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
        _gameOver[playerIndex] = gameOver;
        _playerPanelUis[playerIndex].GameOverText.gameObject.SetActive(gameOver);
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

        for (int i = 0; i < GameConst.MaxPlayers; i++)
        {
            if (_gameOver[i])
                _playerPanelUis[i].GameOverText.gameObject.SetActive(Time.time % 2 > 1);
        }
    }
}