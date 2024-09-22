using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(GameStateSystemGroup), OrderLast = true)]
public partial struct GameStateChangeSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<GameStateData>();
        state.RequireForUpdate<ChangeStateCommand>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
        
        var command = SystemAPI.GetSingleton<ChangeStateCommand>();
        
        foreach (var (gameState, entity) in SystemAPI.Query<RefRW<GameStateData>>().WithEntityAccess())
        {
            ecb.RemoveComponent(entity, gameState.ValueRW.CurrentState);
            ecb.AddComponent(entity, command.TargetState);

            gameState.ValueRW.CurrentState = command.TargetState;
        }
    }
}