using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
[UpdateAfter(typeof(PaddleInputPollSystem))]
public partial class BallStartMovingInputProcessingSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

    protected override void OnCreate()
    {
        _endSimulationEcbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();

        Entities
            .WithNone<LaserPaddleTag>()
            .ForEach((Entity paddle, ref PaddleInputData inputData, in DynamicBuffer<BallLink> ballsBuffer) =>
            {
                if (inputData.Action == InputActionType.Fire)
                {
                    foreach (var ball in ballsBuffer.Reinterpret<Entity>())
                        ecb.AddComponent<BallStartMovingTag>(ball);
                }
            }).Schedule();

        _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }
}