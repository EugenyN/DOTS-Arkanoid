using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup))]
public partial struct BallLossCheckSystem : ISystem
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
        new BallLossCheckJob
        {
            Ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged),
            BallsBufferLookup = SystemAPI.GetBufferLookup<BallLink>(),
            BallLostEventLookup = SystemAPI.GetComponentLookup<BallLostEvent>()
        }.Schedule();
    }
    
    [BurstCompile]
    public partial struct BallLossCheckJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;
        public BufferLookup<BallLink> BallsBufferLookup;
        public ComponentLookup<BallLostEvent> BallLostEventLookup;
        
        private void Execute(Entity entity, in LocalTransform transform, in BallData ballData)
        {
            if (transform.Position.y <= 0)
            {
                BallLostEventLookup.SetComponentEnabled(ballData.OwnerPaddle, true);
                Ecb.DestroyEntity(entity);

                var ballsBuffer = BallsBufferLookup[ballData.OwnerPaddle];
                for (int i = ballsBuffer.Length - 1; i >= 0; i--)
                {
                    if (ballsBuffer[i].Ball == entity)
                        ballsBuffer.RemoveAtSwapBack(i);
                }
            }
        }
    }
}