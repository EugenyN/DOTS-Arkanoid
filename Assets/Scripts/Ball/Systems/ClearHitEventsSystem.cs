using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct ClearHitEventsSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new ClearBallHitEventDynamicBufferJob().Schedule();
        new ClearHitByBallEventJob().Schedule();
        new ClearHitByLaserEventJob().Schedule();
    }
    
    [BurstCompile]
    public partial struct ClearBallHitEventDynamicBufferJob : IJobEntity
    {
        private void Execute(ref DynamicBuffer<BallHitEvent> events)
        {
            events.Clear();
        }
    }
    
    [BurstCompile]
    public partial struct ClearHitByBallEventJob : IJobEntity
    {
        private void Execute(EnabledRefRW<HitByBallEvent> hitByBallEvent)
        {
            hitByBallEvent.ValueRW = false;
        }
    }
    
    [BurstCompile]
    public partial struct ClearHitByLaserEventJob : IJobEntity
    {
        private void Execute(EnabledRefRW<HitByLaserEvent> hitByLaserEvent)
        {
            hitByLaserEvent.ValueRW = false;
        }
    }
}