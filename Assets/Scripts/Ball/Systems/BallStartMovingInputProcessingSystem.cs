using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup))]
public partial struct BallStartMovingInputProcessingSystem : ISystem
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
        
        new BallStartMovingInputProcessingJob
        {
            Ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged)
        }.Schedule();
    }
    
    [BurstCompile]
    [WithNone(typeof(LaserPaddleTag))]
    public partial struct BallStartMovingInputProcessingJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;
        
        private void Execute(ref PaddleInputData inputData, in DynamicBuffer<BallLink> ballsBuffer)
        {
            if (inputData.Action == InputActionType.Fire)
            {
                foreach (var ball in ballsBuffer.Reinterpret<Entity>())
                    Ecb.AddComponent<BallStartMovingTag>(ball);
            }
        }
    }
}