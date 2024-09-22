using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(GameStateSystemGroup))]
public partial struct GameWinCheckSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BallData>();
        state.RequireForUpdate<GameProcessState>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var blocksQuery = SystemAPI.QueryBuilder().WithAll<BlockData>().WithNone<GoldBlock>().Build();
        if (blocksQuery.IsEmpty)
            state.EntityManager.AddSingleFrameComponent(ChangeStateCommand.Create<GameWinState>());
    }
}