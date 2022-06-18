using Unity.Entities;
using Unity.Physics;

public partial class SlowPowerUpSystem : SystemBase
{
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireSingletonForUpdate<PowerUpReceivedEvent>();
    }
    
    protected override void OnUpdate()
    {
        var gameData = GetSingleton<GameData>();
        
        Entities
            .ForEach((Entity paddle, in PowerUpReceivedEvent request, in DynamicBuffer<BallLink> ballsBuffer) =>
            {
                if (request.Type == PowerUpType.Slow)
                {
                    foreach (var ball in ballsBuffer.Reinterpret<Entity>())
                    {
                        var velocity = GetComponent<PhysicsVelocity>(ball);
                        velocity.Linear = MathUtils.ClampMagnitude(velocity.Linear, gameData.BallSpeed);
                        SetComponent(ball, velocity);
                    }
                }
            }).Schedule();
    }
}