using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;

public class TitleUI : MonoBehaviour
{
    [SerializeField] GameObject _onePlayerButton;
    [SerializeField] GameObject _twoPlayerButton;
    
    private GameSystem _gameSystem;

    private void OnEnable()
    {
        if (World.DefaultGameObjectInjectionWorld == null)
            return;
        _gameSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<GameSystem>();
        
        EventSystem.current.SetSelectedGameObject(_onePlayerButton);
    }

    private void Update()
    {
        if (World.DefaultGameObjectInjectionWorld == null)
            return;
        var inputSettingsQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(
            ComponentType.ReadOnly<InputSettings>());
        if (inputSettingsQuery.IsEmptyIgnoreFilter)
            return;
        
        var inputSettings = inputSettingsQuery.GetSingleton<InputSettings>();
        
        for (int i = 0; i < GameConst.MaxPlayers; i++)
        {
            if (Input.GetButtonDown(inputSettings.InputNames[i].Action) || 
                Input.GetButtonDown(inputSettings.InputNames[i].Pause))
            {
                if (EventSystem.current.currentSelectedGameObject == _onePlayerButton)
                    _gameSystem.StartGame(1);
                if (EventSystem.current.currentSelectedGameObject == _twoPlayerButton)
                    _gameSystem.StartGame(2);
            }
        }
    }

    public void OnOnePlayerButtonClick()
    {
        _gameSystem.StartGame(1);
    }

    public void OnTwoPlayerButtonClick()
    {
        _gameSystem.StartGame(2);
    }
    
    public void OnFourPlayerDemoButtonClick()
    {
        _gameSystem.StartGame(4);
    }
}