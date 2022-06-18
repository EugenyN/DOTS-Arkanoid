using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(ExportPhysicsWorld)), UpdateBefore(typeof(EndFramePhysicsSystem))]
public partial class LaserTriggeringSystem : SystemBase
{
    private StepPhysicsWorld _stepPhysicsWorld;
    private EndFixedStepSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
    
    protected override void OnCreate()
    {
        _stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        _endSimulationEcbSystem = World.GetOrCreateSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
        RequireSingletonForUpdate<LaserShotTag>();
    }
    
    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        this.RegisterPhysicsRuntimeSystemReadOnly();
    }

    [BurstCompile]
    private struct LaserTriggeringJob : ICollisionEventsJob
    {
        public EntityCommandBuffer Ecb;
        
        [ReadOnly]
        public ComponentDataFromEntity<LaserShotTag> LaserShots;

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
            LaserShots = GetComponentDataFromEntity<LaserShotTag>(true),
            Ecb = _endSimulationEcbSystem.CreateCommandBuffer()
        }.Schedule(_stepPhysicsWorld.Simulation, Dependency);
        
        _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }
}