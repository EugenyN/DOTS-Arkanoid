using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup))]
public partial struct BallStartMovingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<GameData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var gameData = SystemAPI.GetSingleton<GameData>();

        var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        
        new BallStartMovingJob
        {
            Ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged),
            LocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true),
            PaddleDataLookup = SystemAPI.GetComponentLookup<PaddleData>(true),
            Random = Random.CreateFromIndex(state.GlobalSystemVersion),
            BallSpeed = gameData.BallSpeed
        }.Schedule();
    }
    
    [BurstCompile]
    [WithAll(typeof(BallStartMovingTag), typeof(BallStuckToPaddle))]
    public partial struct BallStartMovingJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;
        [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
        [ReadOnly] public ComponentLookup<PaddleData> PaddleDataLookup;
        public Random Random;
        public float BallSpeed;
        
        private void Execute(Entity ball, in BallData ballData, in LocalTransform transform)
        {
            Ecb.RemoveComponent<BallStuckToPaddle>(ball);
            Ecb.RemoveComponent<BallStartMovingTag>(ball);

            var paddleTransform = LocalTransformLookup[ballData.OwnerPaddle];
            var paddleData = PaddleDataLookup[ballData.OwnerPaddle];
                
            var direction =
                BallsHelper.GetBounceDirection(transform.Position, paddleTransform.Position, paddleData.Size);

            if (direction.Equals(math.up()))
                direction = BallsHelper.GetRandomDirection(Random);

            Ecb.AddComponent(ball, new PhysicsVelocity
            {
                Linear = direction * BallSpeed, Angular = float3.zero
            });

            AudioSystem.PlayAudio(Ecb, AudioClipKeys.PaddleHit);
        }
    }
}