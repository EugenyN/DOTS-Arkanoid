using Unity.Entities;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct OptionsUIPresentationSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<OptionsUI>();
    }

    public void OnUpdate(ref SystemState state)
    {
        GameUtils.TryGetSingletonEntityManaged<OptionsUI>(state.EntityManager, out var optionsUIEntity);
        var optionsUI = state.EntityManager.GetComponentObject<OptionsUI>(optionsUIEntity);
        
        foreach (var command in SystemAPI.Query<ChangeStateCommand>())
            optionsUI.gameObject.SetActive(command.TargetState == typeof(OptionsMenuState));
    }
}