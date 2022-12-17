using Unity.Entities;
using Unity.Physics;

public partial class SlowPowerUpSystem : SystemBase
{
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireForUpdate<PowerUpReceivedEvent>();
    }
    
    protected override void OnUpdate()
    {
        var gameData = SystemAPI.GetSingleton<GameData>();
        
        Entities
            .ForEach((Entity paddle, in PowerUpReceivedEvent request, in DynamicBuffer<BallLink> ballsBuffer) =>
            {
                if (request.Type == PowerUpType.Slow)
                {
                    foreach (var ball in ballsBuffer.Reinterpret<Entity>())
                    {
                        var velocity = SystemAPI.GetComponent<PhysicsVelocity>(ball);
                        velocity.Linear = MathUtils.ClampMagnitude(velocity.Linear, gameData.BallSpeed);
                        SystemAPI.SetComponent(ball, velocity);
                    }
                }
            }).Schedule();
    }
}