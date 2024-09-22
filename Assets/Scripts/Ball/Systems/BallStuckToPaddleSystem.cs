using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup))]
public partial struct BallStuckToPaddleSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<BallStuckToPaddle>();
        state.RequireForUpdate<GameProcessState>();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        
        new BallStuckToPaddleJob
        {
            Ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged),
            LocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(),
            DeltaTime = SystemAPI.Time.DeltaTime
        }.Schedule();
    }
    
    [BurstCompile]
    public partial struct BallStuckToPaddleJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;
        public ComponentLookup<LocalTransform> LocalTransformLookup;
        public float DeltaTime;
        
        private void Execute(Entity entity, ref BallStuckToPaddle stuckData, in BallData data)
        {
            stuckData.StuckTime -= DeltaTime;

            if (stuckData.StuckTime <= 0.0f)
            {
                Ecb.AddComponent(entity, new BallStartMovingTag());
            }
            else
            {
                var paddleTransform = LocalTransformLookup[data.OwnerPaddle];
                LocalTransformLookup[entity] = LocalTransform.FromPosition(
                    paddleTransform.Position + new float3(stuckData.Offset, 1, 0));
            }
        }
    }
}