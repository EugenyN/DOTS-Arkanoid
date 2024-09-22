using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;

public class TitleUI : MonoBehaviour
{
    [SerializeField] GameObject _onePlayerButton;
    [SerializeField] GameObject _twoPlayerButton;
    
    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(_onePlayerButton);
    }

    private void Update()
    {
        if (World.DefaultGameObjectInjectionWorld == null)
            return;
        
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        GameUtils.TryGetSingleton<LevelsSettings>(entityManager, out var levelsSettings);
        GameUtils.TryGetSingletonManaged<InputSettings>(entityManager, out var inputSettings);

        for (int i = 0; i < levelsSettings.MaxPlayers; i++)
        {
            if (Input.GetButtonDown(inputSettings.InputNames[i].Action) || 
                Input.GetButtonDown(inputSettings.InputNames[i].Pause))
            {
                if (EventSystem.current.currentSelectedGameObject == _onePlayerButton)
                    GameSystem.StartGame(entityManager, 1);
                if (EventSystem.current.currentSelectedGameObject == _twoPlayerButton)
                    GameSystem.StartGame(entityManager, 2);
            }
        }
    }

    public void OnOnePlayerButtonClick()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        GameSystem.StartGame(entityManager, 1);
    }

    public void OnTwoPlayerButtonClick()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        GameSystem.StartGame(entityManager, 2);
    }
    
    public void OnFourPlayerDemoButtonClick()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        GameSystem.StartGame(entityManager, 4);
    }
}