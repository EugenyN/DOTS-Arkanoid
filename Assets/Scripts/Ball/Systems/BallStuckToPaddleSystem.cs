using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup))]
public partial class BallStuckToPaddleSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
    
    protected override void OnCreate()
    {
        _endSimulationEcbSystem = World.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();

        RequireForUpdate<BallStuckToPaddle>();
        RequireForUpdate<GameProcessState>();
    }
    
    protected override void OnUpdate()
    {
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();

        var deltaTime = World.Time.DeltaTime;
        
        Entities.ForEach((Entity entity, ref BallStuckToPaddle stuckData, in BallData data) =>
        {
            stuckData.StuckTime -= deltaTime;

            if (stuckData.StuckTime <= 0.0f)
            {
                ecb.AddComponent(entity, new BallStartMovingTag());
            }
            else
            {
                var paddleTransform = SystemAPI.GetComponent<LocalTransform>(data.OwnerPaddle);
                var position = LocalTransform.FromPosition(
                    paddleTransform.Position + new float3(stuckData.Offset, 1, 0));
                SystemAPI.SetComponent(entity, position);
            }
        }).Schedule();
        
        _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }
}