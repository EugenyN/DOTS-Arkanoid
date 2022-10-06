using Unity.Entities;

public partial class GameWinCheckSystem : SystemBase
{
    private EntityQuery _blocksQuery;
    
    protected override void OnCreate()
    {
        base.OnCreate();

        _blocksQuery = GetEntityQuery(new EntityQueryDesc {
            None = new ComponentType[] { typeof(GoldBlock) }, All = new ComponentType[] { typeof(BlockData) }
        });
        
        RequireForUpdate(GetEntityQuery(typeof(BallData)));
        RequireForUpdate<GameProcessState>();
    }

    protected override void OnUpdate()
    {
        if (_blocksQuery.IsEmpty)
            EntityManager.AddSingleFrameComponent(new ChangeStateCommand { TargetState = typeof(GameWinState) });
    }
}