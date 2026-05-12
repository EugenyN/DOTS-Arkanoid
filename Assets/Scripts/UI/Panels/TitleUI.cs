using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TitleUI : MonoBehaviour
{
    [SerializeField] GameObject _onePlayerButton;
    [SerializeField] GameObject _twoPlayerButton;
    [SerializeField] GameObject _optionsButton;
    
    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(_onePlayerButton);
    }

    private void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
                Application.Quit();
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

    public void OnOptionsButtonClick()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        entityManager.AddSingleFrameComponent(ChangeStateCommand.Create<OptionsMenuState>());
    }
    
    public void OnFourPlayerDemoButtonClick()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        GameSystem.StartGame(entityManager, 4);
    }
}