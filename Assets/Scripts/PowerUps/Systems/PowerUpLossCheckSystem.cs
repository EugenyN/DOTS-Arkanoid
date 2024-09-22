using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(PowerUpsSystemGroup))]
public partial struct PowerUpLossCheckSystem : ISystem
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
        
        new PowerUpLossCheckJob
        {
            Ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged)
        }.Schedule();
    }
    
    [BurstCompile]
    [WithAll(typeof(PowerUpData))]
    public partial struct PowerUpLossCheckJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;
        
        private void Execute(Entity entity, in LocalTransform transform)
        {
            if (transform.Position.y <= 0)
                Ecb.DestroyEntity(entity);
        }
    }
}