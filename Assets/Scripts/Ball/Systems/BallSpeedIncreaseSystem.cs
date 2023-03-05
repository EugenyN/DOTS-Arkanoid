using Unity.Entities;
using Unity.Physics;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup))]
public partial class BallSpeedIncreaseSystem : SystemBase
{
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireForUpdate<BallHitEvent>();
    }
    
    protected override void OnUpdate()
    {
        var gameSettings = SystemAPI.GetSingleton<GameSettings>();

        Entities.ForEach((ref PhysicsVelocity velocity, in BallHitEvent hitEvent) =>
        {
            if (SystemAPI.HasComponent<PaddleData>(hitEvent.HitEntity))
                velocity.Linear *= gameSettings.BallSpeedIncreaseFactor;
        }).Schedule();
    }
}