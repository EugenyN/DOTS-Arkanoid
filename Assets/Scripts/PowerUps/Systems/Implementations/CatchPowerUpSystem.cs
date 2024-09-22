using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(PowerUpsSystemGroup))]
public partial struct CatchPowerUpSystem : ISystem
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
            Ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged),
            BallStuckToPaddleLookup = SystemAPI.GetComponentLookup<BallStuckToPaddle>()
        }.Schedule();
    }
    
    [BurstCompile]
    public partial struct BreakPowerUpJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;
        [ReadOnly] public ComponentLookup<BallStuckToPaddle> BallStuckToPaddleLookup;
        
        private void Execute(Entity paddle, in PaddleData paddleData, in PowerUpReceivedEvent request, 
            in DynamicBuffer<BallLink> ballsBuffer)
        {
            if (paddleData.ExclusivePowerUp == request.Type)
                return;

            if (paddleData.ExclusivePowerUp == PowerUpType.Catch && PowerUpsHelper.IsExclusivePowerUp(request.Type))
            {
                Ecb.RemoveComponent<StickPaddleTag>(paddle);
                    
                foreach (var ball in ballsBuffer.Reinterpret<Entity>()) {
                    if (BallStuckToPaddleLookup.HasComponent(ball))
                        Ecb.RemoveComponent<BallStuckToPaddle>(ball);
                }
            }

            if (request.Type == PowerUpType.Catch)
                Ecb.AddComponent(paddle, new StickPaddleTag());
        }
    }
}