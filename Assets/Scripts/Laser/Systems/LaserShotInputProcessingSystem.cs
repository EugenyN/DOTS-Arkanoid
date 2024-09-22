using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup))]
public partial struct LaserShotInputProcessingSystem : ISystem
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
        
        new LaserShotInputProcessingJob
        {
            Ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged)
        }.Schedule();
    }
    
    [BurstCompile]
    [WithAll(typeof(LaserPaddleTag))]
    public partial struct LaserShotInputProcessingJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;
        
        private void Execute(ref PaddleInputData inputData, in OwnerPlayerId ownerPlayerId, in LocalTransform transform)
        {
            if (inputData.Action == InputActionType.Fire)
            {
                Ecb.AddSingleFrameComponent(new LaserSpawnRequest
                {
                    Position = transform.Position + new float3(0, 1, 0), OwnerPlayer = ownerPlayerId.Value
                });
                AudioSystem.PlayAudio(Ecb, AudioClipKeys.LaserShot);
            }
        }
    }
}