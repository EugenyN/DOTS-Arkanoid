using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;

[UpdateInGroup(typeof(PowerUpsSystemGroup), OrderFirst = true)]
public partial struct PowerUpTriggeringSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
        state.RequireForUpdate<PowerUpData>();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Dependency = new PowerUpTriggeringJob
        {
            PowerUpDataLookup = SystemAPI.GetComponentLookup<PowerUpData>(true),
            PaddleDataLookup = SystemAPI.GetComponentLookup<PaddleData>(true),
            PowerUpReceivedEventLookup = SystemAPI.GetComponentLookup<PowerUpReceivedEvent>(),
        }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
    }
    
    [BurstCompile]
    private struct PowerUpTriggeringJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentLookup<PowerUpData> PowerUpDataLookup;
        [ReadOnly] public ComponentLookup<PaddleData> PaddleDataLookup;
        public ComponentLookup<PowerUpReceivedEvent> PowerUpReceivedEventLookup;
        
        public void Execute(TriggerEvent triggerEvent)
        {
            var entityA = triggerEvent.EntityA;
            var entityB = triggerEvent.EntityB;
            
            var powerUpEntity = PowerUpDataLookup.HasComponent(entityA) ? entityA : 
                PowerUpDataLookup.HasComponent(entityB) ? entityB : Entity.Null;
            
            var paddleEntity = PaddleDataLookup.HasComponent(entityA) ? entityA :
                PaddleDataLookup.HasComponent(entityB) ? entityB : Entity.Null;

            if (paddleEntity != Entity.Null && powerUpEntity != Entity.Null)
            {
                PowerUpReceivedEventLookup[paddleEntity] = new PowerUpReceivedEvent {
                    PowerUp = powerUpEntity, Type = PowerUpDataLookup[powerUpEntity].Type
                };
                PowerUpReceivedEventLookup.SetComponentEnabled(paddleEntity, true);
            }
        }
    }
}