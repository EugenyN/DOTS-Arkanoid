using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial class OptionsUIPresentationSystem : SystemBase
{
    private OptionsUI _optionsUI;
    
    protected override void OnUpdate()
    {
        if (_optionsUI == null)
            _optionsUI = Object.FindAnyObjectByType<OptionsUI>(FindObjectsInactive.Include);

        foreach (var command in SystemAPI.Query<ChangeStateCommand>())
            _optionsUI.gameObject.SetActive(command.TargetState == typeof(OptionsMenuState));
    }
}