using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[UpdateBefore(typeof(BallSpawnerSystem))]
public partial class DisruptionPowerUpSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
    
    protected override void OnCreate()
    {
        base.OnCreate();
        
        _endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        
        RequireSingletonForUpdate<PowerUpReceivedEvent>();
    }
    
    protected override void OnUpdate()
    {
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();

        var gameSettings = GetSingleton<GameSettings>();
        var translationFromEntity = GetComponentDataFromEntity<Translation>(true);
        var velocityFromEntity = GetComponentDataFromEntity<PhysicsVelocity>(true);
        
        Entities
            .WithReadOnly(translationFromEntity)
            .WithReadOnly(velocityFromEntity)
            .ForEach((Entity paddle, in PowerUpReceivedEvent request, in OwnerPlayerId ownerPlayerId,
                in DynamicBuffer<BallLink> ballsBuffer) =>
            {
                if (request.Type == PowerUpType.Disruption) {
                    ActivatePowerUp(paddle, ownerPlayerId.Value, ballsBuffer, ecb, gameSettings,
                        translationFromEntity, velocityFromEntity);
                }
            }).Schedule();
        
        _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }

    private static void ActivatePowerUp(Entity paddle, Entity player, DynamicBuffer<BallLink> ballsBuffer,
        EntityCommandBuffer ecb, GameSettings gameSettings, ComponentDataFromEntity<Translation> translationFromEntity,
        ComponentDataFromEntity<PhysicsVelocity> velocityFromEntity)
    {
        var balls = ballsBuffer.Reinterpret<Entity>();
        if (balls.Length == 0)
            return;

        var ball = balls[0];

        float angle = math.radians(10);
        for (int i = 0; i < gameSettings.DisruptionPowerUpBallsCount; i++)
        {
            ecb.AddSingleFrameComponent(new BallSpawnRequest
            {
                Position = translationFromEntity[ball].Value,
                OwnerPaddle = paddle,
                OwnerPlayer = player,
                Velocity = math.mul(quaternion.RotateZ(-angle + i * angle * 2), velocityFromEntity[ball].Linear)
            });
        }
    }
}