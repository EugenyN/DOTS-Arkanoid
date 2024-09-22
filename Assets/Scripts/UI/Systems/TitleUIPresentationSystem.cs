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
        {
            if (command.TargetState == typeof(MainMenuState))
                titleUI.gameObject.SetActive(true);
            else if (command.TargetState == typeof(GameProcessState))
                titleUI.gameObject.SetActive(false);
        }
    }
}