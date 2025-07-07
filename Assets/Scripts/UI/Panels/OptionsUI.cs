using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour
{
    [SerializeField] Toggle _vsyncToggle;
    [SerializeField] Text _vsyncToggleText;
    [SerializeField] GameObject _backButton;
    [SerializeField] Slider _mouseSensSlider;

    private void Start()
    {
        _vsyncToggle.onValueChanged.AddListener(OnToggleValueChanged);
        _mouseSensSlider.onValueChanged.AddListener(OnMouseSensitivitySliderValueChanged);
        
        float mouseSensitivity = PlayerPrefs.GetFloat("mouseSensitivity", 0.5f);
        _mouseSensSlider.value = mouseSensitivity;
        
        bool vSyncEnabled = PlayerPrefs.GetInt("vSyncEnabled", 1) == 1;
        _vsyncToggle.isOn = vSyncEnabled;
        _vsyncToggleText.text = vSyncEnabled ? "ON" : "OFF";
        
#if UNITY_ANDROID
        _vsyncToggle.enabled = false;
        _vsyncToggleText.text = "UNAVAILABLE";
#endif
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
            OnBackButtonClick();
    }

    private void OnMouseSensitivitySliderValueChanged(float value)
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        GameUtils.TryGetSingletonManaged<InputSettings>(entityManager, out var inputSettings);

        inputSettings.MouseSensitivity = value;
        
        PlayerPrefs.SetFloat("mouseSensitivity", value);
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