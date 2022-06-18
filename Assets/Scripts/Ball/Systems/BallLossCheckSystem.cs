using Unity.Entities;
using Unity.Transforms;

public partial class BallLossCheckSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
    
    protected override void OnCreate()
    {
        _endSimulationEcbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }
    
    protected override void OnUpdate()
    {
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();

        Entities.ForEach((Entity entity, in Translation trans, in BallData ballData) =>
        {
            if (trans.Value.y <= 0)
            {
                ecb.AddSingleFrameComponent(ballData.OwnerPaddle, new BallLostEvent());
                ecb.DestroyEntity(entity);

                var ballsBuffer = GetBuffer<BallLink>(ballData.OwnerPaddle);
                for (int i = ballsBuffer.Length - 1; i >= 0; i--)
                {
                    if (ballsBuffer[i].Ball == entity)
                        ballsBuffer.RemoveAtSwapBack(i);
                }
            }
        }).Schedule();
        
        _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }
}