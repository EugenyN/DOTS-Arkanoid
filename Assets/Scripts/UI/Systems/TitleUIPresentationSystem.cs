using Unity.Entities;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial class TitleUIPresentationSystem : SystemBase
{
    private EntityQuery _titleUIQuery;
    private TitleUI _titleUI;

    protected override void OnCreate()
    {
        base.OnCreate();

        _titleUIQuery = GetEntityQuery(typeof(TitleUI));

        RequireForUpdate<TitleUI>();
    }

    protected override void OnUpdate()
    {
        if (_titleUI == null)
            _titleUI = EntityManager.GetComponentObject<TitleUI>(_titleUIQuery.GetSingletonEntity());

        Entities
            .WithoutBurst()
            .ForEach((in ChangeStateCommand command) =>
            {
                if (command.TargetState == typeof(MainMenuState))
                    _titleUI.gameObject.SetActive(true);
                else if (command.TargetState == typeof(GameProcessState))
                    _titleUI.gameObject.SetActive(false);
            }).Run();
    }
}