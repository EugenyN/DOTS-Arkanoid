using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[UpdateInGroup(typeof(PowerUpsSystemGroup))]
public partial struct DisruptionPowerUpSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<GameSettings>();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var gameSettings = SystemAPI.GetSingleton<GameSettings>();
        
        var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        
        new DisruptionPowerUpJob
        {
            Ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged),
            LocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true),
            PhysicsVelocityLookup = SystemAPI.GetComponentLookup<PhysicsVelocity>(true),
            DisruptionPowerUpBallsCount = gameSettings.DisruptionPowerUpBallsCount
        }.Schedule();
    }
    
    [BurstCompile]
    public partial struct DisruptionPowerUpJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;
        [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
        [ReadOnly] public ComponentLookup<PhysicsVelocity> PhysicsVelocityLookup;
        public int DisruptionPowerUpBallsCount;
        
        private void Execute(Entity paddle, in PowerUpReceivedEvent request, in OwnerPlayerId ownerPlayerId,
            in DynamicBuffer<BallLink> ballsBuffer)
        {
            if (request.Type == PowerUpType.Disruption)
            {
                var balls = ballsBuffer.Reinterpret<Entity>();
                if (balls.Length == 0)
                    return;

                var ball = balls[0];

                float angle = math.radians(10);
                for (int i = 0; i < DisruptionPowerUpBallsCount; i++)
                {
                    Ecb.AddSingleFrameComponent(new BallSpawnRequest
                    {
                        Position = LocalTransformLookup[ball].Position,
                        OwnerPaddle = paddle,
                        OwnerPlayer = ownerPlayerId.Value,
                        Velocity = math.mul(quaternion.RotateZ(-angle + i * angle * 2), PhysicsVelocityLookup[ball].Linear)
                    });
                }
            }
        }
    }
}