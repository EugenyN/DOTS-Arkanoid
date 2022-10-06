using Unity.Entities;

public partial class LaserPowerUpSystem : SystemBase
{
    private const int NormalAnimationFrame = 4;
    private const int LaserAnimationFrame = 16;
    
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
    
    protected override void OnCreate()
    {
        base.OnCreate();
        
        _endSimulationEcbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        
        RequireForUpdate<PowerUpReceivedEvent>();
    }
    
    protected override void OnUpdate()
    {
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();
        
        Entities
            .ForEach((Entity paddle, ref PlayTextureAnimation playTextureAnimation,
                in PaddleData paddleData, in PowerUpReceivedEvent request, in PlayerIndex playerIndex) =>
            {
                if (paddleData.ExclusivePowerUp == request.Type)
                    return;

                if (paddleData.ExclusivePowerUp == PowerUpType.Laser && PowerUpsHelper.IsExclusivePowerUp(request.Type))
                {
                    ecb.RemoveComponent<LaserPaddleTag>(paddle);
                    
                    playTextureAnimation.StartFrame = NormalAnimationFrame * playerIndex.Value;
                    playTextureAnimation.Initialized = false;
                }

                if (request.Type == PowerUpType.Laser)
                {
                    ecb.AddComponent(paddle, new LaserPaddleTag());
                    
                    playTextureAnimation.StartFrame = LaserAnimationFrame;
                    playTextureAnimation.Initialized = false;
                }
            }).Schedule();
        
        _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }
}