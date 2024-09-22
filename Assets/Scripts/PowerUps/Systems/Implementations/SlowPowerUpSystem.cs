using Unity.Burst;
using Unity.Entities;
using Unity.Physics;

[UpdateInGroup(typeof(PowerUpsSystemGroup))]
public partial struct SlowPowerUpSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameData>();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var gameData = SystemAPI.GetSingleton<GameData>();
        
        new SlowPowerUpJob
        {
            PhysicsVelocityLookup = SystemAPI.GetComponentLookup<PhysicsVelocity>(),
            BallSpeed = gameData.BallSpeed
        }.Schedule();
    }
    
    [BurstCompile]
    public partial struct SlowPowerUpJob : IJobEntity
    {
        public ComponentLookup<PhysicsVelocity> PhysicsVelocityLookup;
        public float BallSpeed;
        
        private void Execute(in PowerUpReceivedEvent request, in DynamicBuffer<BallLink> ballsBuffer)
        {
            if (request.Type == PowerUpType.Slow)
            {
                foreach (var ball in ballsBuffer.Reinterpret<Entity>())
                {
                    var velocity = PhysicsVelocityLookup[ball];
                    velocity.Linear = MathUtils.ClampMagnitude(velocity.Linear, BallSpeed * 0.75f);
                    PhysicsVelocityLookup[ball] = velocity;
                }
            }
        }
    }
}