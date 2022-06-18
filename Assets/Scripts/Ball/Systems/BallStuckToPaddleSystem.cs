using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial class BallStuckToPaddleSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
    
    protected override void OnCreate()
    {
        _endSimulationEcbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();

        RequireSingletonForUpdate<BallStuckToPaddle>();
        RequireSingletonForUpdate<GameProcessState>();
    }
    
    protected override void OnUpdate()
    {
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();

        var deltaTime = Time.DeltaTime;
        
        Entities.ForEach((Entity entity, ref BallStuckToPaddle stuckData, in BallData data) =>
        {
            stuckData.StuckTime -= deltaTime;

            if (stuckData.StuckTime <= 0.0f)
            {
                ecb.AddComponent(entity, new BallStartMovingTag());
            }
            else
            {
                var paddlePosition = GetComponent<Translation>(data.OwnerPaddle);
                SetComponent(entity, new Translation {
                    Value = paddlePosition.Value + new float3(stuckData.Offset, 1, 0)
                });
            }
        }).Schedule();
        
        _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }
}