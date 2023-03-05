using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup))]
public partial class LaserShotInputProcessingSystem : SystemBase
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
            .WithAll<LaserPaddleTag>()
            .ForEach((Entity paddle, ref PaddleInputData inputData, in OwnerPlayerId ownerPlayerId,
                in LocalTransform transform) =>
            {
                if (inputData.Action == InputActionType.Fire)
                {
                    ecb.AddSingleFrameComponent(new LaserSpawnRequest
                    {
                        Position = transform.Position + new float3(0, 1, 0), OwnerPlayer = ownerPlayerId.Value
                    });
                    AudioSystem.PlayAudio(ecb, AudioClipKeys.LaserShot);
                }
            }).Schedule();

        _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }
}