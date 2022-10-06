using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;

public partial class PowerUpTriggeringSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

    protected override void OnCreate()
    {
        _endSimulationEcbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();

        RequireForUpdate(GetEntityQuery(typeof(PowerUpData)));
    }

    [BurstCompile]
    private struct PowerUpTriggeringJob : ICollisionEventsJob
    {
        public EntityCommandBuffer Ecb;
        
        [ReadOnly]
        public ComponentLookup<PowerUpData> PowerUps;
        [ReadOnly]
        public ComponentLookup<PaddleData> Paddles;
        
        public void Execute(CollisionEvent collisionEvent)
        {
            var entityA = collisionEvent.EntityA;
            var entityB = collisionEvent.EntityB;
            
            var powerUpEntity = PowerUps.HasComponent(entityA) ? entityA : 
                PowerUps.HasComponent(entityB) ? entityB : Entity.Null;
            
            var paddleEntity = Paddles.HasComponent(entityA) ? entityA :
                Paddles.HasComponent(entityB) ? entityB : Entity.Null;

            if (paddleEntity != Entity.Null && powerUpEntity != Entity.Null)
            {
                Ecb.AddSingleFrameComponent(paddleEntity, new PowerUpReceivedEvent { 
                    PowerUp = powerUpEntity, Type = PowerUps[powerUpEntity].Type
                });
            }
        }
    }

    protected override void OnUpdate()
    {
        Dependency = new PowerUpTriggeringJob
        {
            PowerUps = GetComponentLookup<PowerUpData>(true),
            Paddles = GetComponentLookup<PaddleData>(true),
            Ecb = _endSimulationEcbSystem.CreateCommandBuffer()
        }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), Dependency);
        
        _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }
}