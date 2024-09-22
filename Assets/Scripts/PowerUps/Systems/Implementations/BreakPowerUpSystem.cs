using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(PowerUpsSystemGroup))]
public partial struct BreakPowerUpSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        
        new BreakPowerUpJob
        {
            Ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged)
        }.Schedule();
    }
    
    [BurstCompile]
    public partial struct BreakPowerUpJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;
        
        private void Execute(in PowerUpReceivedEvent request)
        {
            if (request.Type == PowerUpType.Break)
            {
                Ecb.AddSingleFrameComponent(ChangeStateCommand.Create<GameWinState>());
            }
        }
    }
}