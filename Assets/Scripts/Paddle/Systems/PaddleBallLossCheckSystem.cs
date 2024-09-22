using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup))]
public partial struct PaddleBallLossCheckSystem : ISystem
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
        
        new PaddleBallHitJob
        {
            Ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged)
        }.Schedule();
    }
    
    [BurstCompile]
    [WithAll(typeof(BallLostEvent))]
    public partial struct PaddleBallHitJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;
        
        private void Execute(Entity paddle, in DynamicBuffer<BallLink> ballsBuffer, 
            EnabledRefRW<BallLostEvent> ballLostEvent)
        {
            if (ballsBuffer.IsEmpty)
                Ecb.AddComponent(paddle, new PaddleDyingStateData());
            ballLostEvent.ValueRW = false;
        }
    }
}