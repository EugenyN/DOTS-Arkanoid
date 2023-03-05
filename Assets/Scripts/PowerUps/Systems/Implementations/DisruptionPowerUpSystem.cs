using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[UpdateInGroup(typeof(PowerUpsSystemGroup))]
public partial class DisruptionPowerUpSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
    
    protected override void OnCreate()
    {
        base.OnCreate();
        
        _endSimulationEcbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        
        RequireForUpdate<PowerUpReceivedEvent>();
    }
    
    protected override void OnUpdate()
    {
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();

        var gameSettings = SystemAPI.GetSingleton<GameSettings>();
        var transformFromEntity = GetComponentLookup<LocalTransform>(true);
        var velocityFromEntity = GetComponentLookup<PhysicsVelocity>(true);
        
        Entities
            .WithReadOnly(transformFromEntity)
            .WithReadOnly(velocityFromEntity)
            .ForEach((Entity paddle, in PowerUpReceivedEvent request, in OwnerPlayerId ownerPlayerId,
                in DynamicBuffer<BallLink> ballsBuffer) =>
            {
                if (request.Type == PowerUpType.Disruption) {
                    ActivatePowerUp(paddle, ownerPlayerId.Value, ballsBuffer, ecb, gameSettings,
                        transformFromEntity, velocityFromEntity);
                }
            }).Schedule();
        
        _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }

    private static void ActivatePowerUp(Entity paddle, Entity player, DynamicBuffer<BallLink> ballsBuffer,
        EntityCommandBuffer ecb, GameSettings gameSettings, ComponentLookup<LocalTransform> transformFromEntity,
        ComponentLookup<PhysicsVelocity> velocityFromEntity)
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
                Position = transformFromEntity[ball].Position,
                OwnerPaddle = paddle,
                OwnerPlayer = player,
                Velocity = math.mul(quaternion.RotateZ(-angle + i * angle * 2), velocityFromEntity[ball].Linear)
            });
        }
    }
}