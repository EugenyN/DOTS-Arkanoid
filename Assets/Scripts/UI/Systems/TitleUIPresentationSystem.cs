using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial class TitleUIPresentationSystem : SystemBase
{
    private TitleUI _titleUI;

    protected override void OnUpdate()
    {
        if (_titleUI == null)
            _titleUI = Object.FindAnyObjectByType<TitleUI>(FindObjectsInactive.Include);

        foreach (var command in SystemAPI.Query<ChangeStateCommand>())
            _titleUI.gameObject.SetActive(command.TargetState == typeof(MainMenuState));
    }
}