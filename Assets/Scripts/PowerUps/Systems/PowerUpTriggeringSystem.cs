using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

public partial class PowerUpTriggeringSystem : SystemBase
{
    private StepPhysicsWorld _stepPhysicsWorld;
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

    protected override void OnCreate()
    {
        _stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        _endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        RequireForUpdate(GetEntityQuery(typeof(PowerUpData)));
    }

    [BurstCompile]
    private struct PowerUpTriggeringJob : ICollisionEventsJob
    {
        public EntityCommandBuffer Ecb;
        
        [ReadOnly]
        public ComponentDataFromEntity<PowerUpData> PowerUps;
        [ReadOnly]
        public ComponentDataFromEntity<PaddleData> Paddles;
        
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
            PowerUps = GetComponentDataFromEntity<PowerUpData>(true),
            Paddles = GetComponentDataFromEntity<PaddleData>(true),
            Ecb = _endSimulationEcbSystem.CreateCommandBuffer()
        }.Schedule(_stepPhysicsWorld.Simulation, Dependency);
        
        _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }
}