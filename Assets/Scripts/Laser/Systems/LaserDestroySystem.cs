using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup))]
public partial struct LaserDestroySystem : ISystem
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
        
        new LaserDestroyJob
        {
            Ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged)
        }.Schedule();
    }
    
    [BurstCompile]
    public partial struct LaserDestroyJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;
        
        private void Execute(in HitByLaserEvent hitByLaserEvent)
        {
            Ecb.DestroyEntity(hitByLaserEvent.LaserShot);
        }
    }
}