using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

public partial class PowerUpLossCheckSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
    
    protected override void OnCreate()
    {
        base.OnCreate();
        
        _endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }
    
    protected override void OnUpdate()
    {
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();

        Entities.WithAll<PowerUpData>().ForEach((Entity entity, in Translation trans) =>
        {
            if (trans.Value.y <= 0)
                ecb.DestroyEntity(entity);
        }).Schedule();
        
        _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }
}