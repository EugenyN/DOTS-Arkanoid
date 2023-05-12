using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
public partial class LaserTriggeringSystem : SystemBase
{
    private EndFixedStepSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
    
    protected override void OnCreate()
    {
        _endSimulationEcbSystem = World.GetOrCreateSystemManaged<EndFixedStepSimulationEntityCommandBufferSystem>();
        RequireForUpdate<LaserShotTag>();
    }

    [BurstCompile]
    private struct LaserTriggeringJob : ITriggerEventsJob
    {
        public EntityCommandBuffer Ecb;
        
        [ReadOnly]
        public ComponentLookup<LaserShotTag> LaserShots;

        public void Execute(TriggerEvent triggerEvent)
        {
            var entityA = triggerEvent.EntityA;
            var entityB = triggerEvent.EntityB;
            
            if (LaserShots.HasComponent(entityA))
                Ecb.AddSingleFrameComponent(entityB, new HitByLaserEvent { LaserShot = entityA });
            
            if (LaserShots.HasComponent(entityB))
                Ecb.AddSingleFrameComponent(entityA, new HitByLaserEvent { LaserShot = entityB });
        }
    }

    protected override void OnUpdate()
    {
        Dependency = new LaserTriggeringJob
        {
            LaserShots = GetComponentLookup<LaserShotTag>(true),
            Ecb = _endSimulationEcbSystem.CreateCommandBuffer()
        }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), Dependency);
        
        _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }
}