using Unity.Entities;

public partial class CatchPowerUpSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
    
    protected override void OnCreate()
    {
        base.OnCreate();
        
        _endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        
        RequireSingletonForUpdate<PowerUpReceivedEvent>();
    }
    
    protected override void OnUpdate()
    {
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();
        
        Entities
            .ForEach((Entity paddle, in PaddleData paddleData, in PowerUpReceivedEvent request, 
                in DynamicBuffer<BallLink> ballsBuffer) =>
            {
                if (paddleData.ExclusivePowerUp == request.Type)
                    return;

                if (paddleData.ExclusivePowerUp == PowerUpType.Catch && PowerUpsHelper.IsExclusivePowerUp(request.Type))
                {
                    ecb.RemoveComponent<StickPaddleTag>(paddle);
                    
                    foreach (var ball in ballsBuffer.Reinterpret<Entity>()) {
                        if (HasComponent<BallStuckToPaddle>(ball))
                            ecb.RemoveComponent<BallStuckToPaddle>(ball);
                    }
                }

                if (request.Type == PowerUpType.Catch)
                    ecb.AddComponent(paddle, new StickPaddleTag());
            }).Schedule();
        
        _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }
}