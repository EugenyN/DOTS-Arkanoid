using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour
{
    [SerializeField] Toggle _vsyncToggle;
    [SerializeField] Text _vsyncToggleText;
    [SerializeField] GameObject _backButton;
    [SerializeField] Slider _inputSensSlider;

    private void Start()
    {
        _vsyncToggle.onValueChanged.AddListener(OnToggleValueChanged);
        _inputSensSlider.onValueChanged.AddListener(OnInputSensitivitySliderValueChanged);
        
        float mouseSensitivity = PlayerPrefs.GetFloat("inputSensitivity", 0.5f);
        _inputSensSlider.value = mouseSensitivity;
        
        bool vSyncEnabled = PlayerPrefs.GetInt("vSyncEnabled", 1) == 1;
        _vsyncToggle.isOn = vSyncEnabled;
        _vsyncToggleText.text = vSyncEnabled ? "ON" : "OFF";
        
#if UNITY_ANDROID
        _vsyncToggle.enabled = false;
        _vsyncToggleText.text = "UNAVAILABLE";
#endif
    }
    
    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(_vsyncToggle.gameObject);
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            OnBackButtonClick();
    }

    private void OnInputSensitivitySliderValueChanged(float value)
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        GameUtils.TryGetSingletonRW<GameSettings>(entityManager, out var gameSettings);

        gameSettings.ValueRW.InputSensitivity = value;
        
        PlayerPrefs.SetFloat("inputSensitivity", value);
    }

    private void OnToggleValueChanged(bool isOn)
    {
        GameSystem.ApplyGraphicsSettings(isOn);
        _vsyncToggleText.text = isOn ? "ON" : "OFF";
        PlayerPrefs.SetInt("vSyncEnabled", isOn ? 1 : 0);
    }
    
    public void OnBackButtonClick()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        entityManager.AddSingleFrameComponent(ChangeStateCommand.Create<MainMenuState>());
    }
}