using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup))]
public partial class PaddleBallLossCheckSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
    
    protected override void OnCreate()
    {
        _endSimulationEcbSystem = World.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
    }
    
    protected override void OnUpdate()
    {
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();

        Entities
            .WithAny<BallLostEvent>()
            .ForEach((Entity paddle, in DynamicBuffer<BallLink> ballsBuffer) =>
            {
                if (ballsBuffer.IsEmpty)
                    ecb.AddComponent(paddle, new PaddleDyingStateData());
            }).Schedule();
        
        _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }
}