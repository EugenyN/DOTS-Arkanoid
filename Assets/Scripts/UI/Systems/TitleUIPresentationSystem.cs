using Unity.Entities;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct TitleUIPresentationSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<TitleUI>();
    }

    public void OnUpdate(ref SystemState state)
    {
        GameUtils.TryGetSingletonEntityManaged<TitleUI>(state.EntityManager, out var titleUIEntity);
        var titleUI = state.EntityManager.GetComponentObject<TitleUI>(titleUIEntity);
        
        foreach (var command in SystemAPI.Query<ChangeStateCommand>())
            titleUI.gameObject.SetActive(command.TargetState == typeof(MainMenuState));
    }
}