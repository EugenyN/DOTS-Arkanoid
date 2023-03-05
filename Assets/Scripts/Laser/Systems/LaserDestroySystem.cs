using Unity.Entities;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup))]
public partial class LaserDestroySystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

    protected override void OnCreate()
    {
        _endSimulationEcbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
    }
    
    protected override void OnUpdate()
    {
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();

        Entities.ForEach((Entity entity, in HitByLaserEvent hitByLaser) =>
        {
            ecb.DestroyEntity(hitByLaser.LaserShot);
        }).Schedule();
        
        _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }
}