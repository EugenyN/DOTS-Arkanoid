using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
public partial struct LaserTriggeringSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
        state.RequireForUpdate<LaserShotTag>();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Dependency = new LaserTriggeringJob
        {
            LaserShots = SystemAPI.GetComponentLookup<LaserShotTag>(true),
            HitByLaserEventLookup = SystemAPI.GetComponentLookup<HitByLaserEvent>(),
        }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
    }
    
    [BurstCompile]
    private struct LaserTriggeringJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentLookup<LaserShotTag> LaserShots;
        public ComponentLookup<HitByLaserEvent> HitByLaserEventLookup;

        public void Execute(TriggerEvent triggerEvent)
        {
            var entityA = triggerEvent.EntityA;
            var entityB = triggerEvent.EntityB;

            if (LaserShots.HasComponent(entityA))
            {
                HitByLaserEventLookup[entityB] = new HitByLaserEvent { LaserShot = entityA };
                HitByLaserEventLookup.SetComponentEnabled(entityB, true);
            }

            if (LaserShots.HasComponent(entityB))
            {
                HitByLaserEventLookup[entityA] = new HitByLaserEvent { LaserShot = entityB };
                HitByLaserEventLookup.SetComponentEnabled(entityA, true);
            }
        }
    }
}