using Unity.Entities;
using Unity.Physics;

public partial class BallSpeedIncreaseSystem : SystemBase
{
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireSingletonForUpdate<BallHitEvent>();
    }
    
    protected override void OnUpdate()
    {
        var gameSettings = GetSingleton<GameSettings>();

        Entities.ForEach((ref PhysicsVelocity velocity, in BallHitEvent hitEvent) =>
        {
            if (HasComponent<PaddleData>(hitEvent.HitEntity))
                velocity.Linear *= gameSettings.BallSpeedIncreaseFactor;
        }).Schedule();
    }
}