using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup))]
public partial struct BallSpeedIncreaseSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameSettings>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var gameSettings = SystemAPI.GetSingleton<GameSettings>();
        
        new BallSpeedIncreaseJob
        {
            PaddleDataLookup = SystemAPI.GetComponentLookup<PaddleData>(true),
            BallSpeedIncreaseFactor = gameSettings.BallSpeedIncreaseFactor
        }.Schedule();
    }
    
    [BurstCompile]
    public partial struct BallSpeedIncreaseJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<PaddleData> PaddleDataLookup;
        public float BallSpeedIncreaseFactor;
        
        private void Execute(ref PhysicsVelocity velocity, in DynamicBuffer<BallHitEvent> ballHitEvents)
        {
            foreach (var ballHitEvent in ballHitEvents)
            {
                if (PaddleDataLookup.HasComponent(ballHitEvent.HitEntity))
                    velocity.Linear *= BallSpeedIncreaseFactor;
            }
        }
    }
}