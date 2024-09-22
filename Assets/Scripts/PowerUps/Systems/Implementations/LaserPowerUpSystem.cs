using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(PowerUpsSystemGroup))]
public partial struct LaserPowerUpSystem : ISystem
{
    private const int NormalAnimationFrame = 4;
    private const int LaserAnimationFrame = 16;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        
        new LaserPowerUpJob
        {
            Ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged)
        }.Schedule();
    }
    
    [BurstCompile]
    public partial struct LaserPowerUpJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;
        
        private void Execute(Entity paddle, ref PlayTextureAnimation playTextureAnimation,
            in PaddleData paddleData, in PowerUpReceivedEvent request, in PlayerIndex playerIndex)
        {
            if (paddleData.ExclusivePowerUp == request.Type)
                return;

            if (paddleData.ExclusivePowerUp == PowerUpType.Laser && PowerUpsHelper.IsExclusivePowerUp(request.Type))
            {
                Ecb.RemoveComponent<LaserPaddleTag>(paddle);
                    
                playTextureAnimation.StartFrame = NormalAnimationFrame * playerIndex.Value;
                playTextureAnimation.Initialized = false;
            }

            if (request.Type == PowerUpType.Laser)
            {
                Ecb.AddComponent(paddle, new LaserPaddleTag());
                    
                playTextureAnimation.StartFrame = LaserAnimationFrame;
                playTextureAnimation.Initialized = false;
            }
        }
    }
}