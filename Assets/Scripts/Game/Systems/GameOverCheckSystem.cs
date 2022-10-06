using Unity.Entities;

public partial class GameOverCheckSystem : SystemBase
{
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireForUpdate<GameProcessState>();
        RequireForUpdate<PlayerData>();
    }

    protected override void OnUpdate()
    {
        bool anyAlive = false;

        Entities.ForEach((in PlayerData playerData) =>
        {
            anyAlive |= playerData.Lives != 0;
        }).Run();

        if (!anyAlive)
        {
            EntityManager.AddSingleFrameComponent(new ChangeStateCommand { TargetState = typeof(GameOverState) });
        }
    }
}