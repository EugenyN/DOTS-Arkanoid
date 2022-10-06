using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
//[UpdateAfter(typeof(ExportPhysicsWorld)), UpdateBefore(typeof(EndFramePhysicsSystem))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
public partial class LaserTriggeringSystem : SystemBase
{
    private EndFixedStepSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
    
    protected override void OnCreate()
    {
        _endSimulationEcbSystem = World.GetOrCreateSystemManaged<EndFixedStepSimulationEntityCommandBufferSystem>();
        RequireForUpdate<LaserShotTag>();
    }
    
    // protected override void OnStartRunning()
    // {
    //     base.OnStartRunning();
    //     this.RegisterPhysicsRuntimeSystemReadOnly();
    // }

    [BurstCompile]
    private struct LaserTriggeringJob : ICollisionEventsJob
    {
        public EntityCommandBuffer Ecb;
        
        [ReadOnly]
        public ComponentLookup<LaserShotTag> LaserShots;

        public void Execute(CollisionEvent collisionEvent)
        {
            var entityA = collisionEvent.EntityA;
            var entityB = collisionEvent.EntityB;
            
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