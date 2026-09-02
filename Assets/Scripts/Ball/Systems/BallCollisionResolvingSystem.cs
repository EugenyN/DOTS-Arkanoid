using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
public partial struct BallCollisionResolvingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var colliderCastHits = new NativeList<ColliderCastHit>(Allocator.TempJob);

        state.Dependency = new BallCollisionResolvingJob
        {
            ColliderCastHits = colliderCastHits,
            World = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld,
            DeltaTime = SystemAPI.Time.DeltaTime,
            LocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(),
            PaddleDataLookup = SystemAPI.GetComponentLookup<PaddleData>(true),
            WallTagLookup = SystemAPI.GetComponentLookup<WallTag>(true),
            BlockDataLookup = SystemAPI.GetComponentLookup<BlockData>(true),
            MegaBallTagLookup = SystemAPI.GetComponentLookup<MegaBallTag>(true),
            GoldBlockLookup = SystemAPI.GetComponentLookup<GoldBlock>(true),
            HitByBallEventLookup = SystemAPI.GetComponentLookup<HitByBallEvent>()
        }.Schedule(state.Dependency);

        state.Dependency = colliderCastHits.Dispose(state.Dependency);
    }

    [BurstCompile]
    [WithAny(typeof(BallData))]
    public partial struct BallCollisionResolvingJob : IJobEntity
    {
        public NativeList<ColliderCastHit> ColliderCastHits;
        public float DeltaTime;
        
        public ComponentLookup<LocalTransform> LocalTransformLookup;
        
        [ReadOnly] public CollisionWorld World;
        [ReadOnly] public ComponentLookup<PaddleData> PaddleDataLookup;
        [ReadOnly] public ComponentLookup<WallTag> WallTagLookup;
        [ReadOnly] public ComponentLookup<BlockData> BlockDataLookup;
        [ReadOnly] public ComponentLookup<MegaBallTag> MegaBallTagLookup;
        [ReadOnly] public ComponentLookup<GoldBlock> GoldBlockLookup;
        
        public ComponentLookup<HitByBallEvent> HitByBallEventLookup;

        private void Execute(Entity ball, ref PhysicsVelocity velocity, in PhysicsCollider collider,
            ref DynamicBuffer<BallHitEvent> ballHitEvents)
        {
            ColliderCastHits.Clear();

            var transform = LocalTransformLookup[ball];
            
            unsafe
            {
                var colliderCastInput = new ColliderCastInput {
                    Collider = collider.ColliderPtr, 
                    Start = transform.Position - velocity.Linear * DeltaTime, End = transform.Position
                };

                World.CastCollider(colliderCastInput, ref ColliderCastHits);
            }

            if (ColliderCastHits.IsCreated && ColliderCastHits.Length != 0)
            {
                float closestHitFraction = float.MaxValue;
                ColliderCastHit closestHit = default;

                for (int i = 0; i < ColliderCastHits.Length; i++)
                {
                    var hit = ColliderCastHits[i];
                    
                    HitByBallEventLookup[hit.Entity] = new HitByBallEvent { Ball = ball };
                    HitByBallEventLookup.SetComponentEnabled(hit.Entity, true);
                    
                    ballHitEvents.Add(new BallHitEvent { HitEntity = hit.Entity });

                    if (hit.Fraction < closestHitFraction)
                    {
                        closestHitFraction = hit.Fraction;
                        closestHit = hit;
                    }
                }
                
                if (PaddleDataLookup.HasComponent(closestHit.Entity))
                {
                    var paddleData = PaddleDataLookup[closestHit.Entity];
                    var hitTransform = LocalTransformLookup[closestHit.Entity];
                    
                    if (ResolvePaddleCollision(ref velocity, ref transform, hitTransform.Position, paddleData.Size))
                        LocalTransformLookup[ball] = transform;
                }
                else if (WallTagLookup.HasComponent(closestHit.Entity))
                {
                    ResolveBallCollision(ref velocity, ref transform, closestHit, DeltaTime);
                    LocalTransformLookup[ball] = transform;
                }
                else if (BlockDataLookup.HasComponent(closestHit.Entity))
                {
                    if (MegaBallTagLookup.HasComponent(ball) && !GoldBlockLookup.HasComponent(closestHit.Entity))
                    {
                        // ignore
                    }
                    else
                    {
                        ResolveBallCollision(ref velocity, ref transform, closestHit, DeltaTime);
                        LocalTransformLookup[ball] = transform;
                    }
                }
            }
        }
    }

    private static bool ResolvePaddleCollision(ref PhysicsVelocity ballVelocity, ref LocalTransform ballTransform, 
        float3 paddlePosition, float3 paddleSize)
    {
        if (ballVelocity.Linear.y > 0)
            return false;

        var ballPosition = ballTransform.Position;
        if (ballPosition.y < paddlePosition.y - paddleSize.y / 2.0f)
            return false;

        var direction = BallsHelper.GetBounceDirection(ballPosition, paddlePosition, paddleSize);
        ballVelocity.Linear = direction * math.length(ballVelocity.Linear);
        
        var minY = paddlePosition.y + paddleSize.y / 2f + 0.05f;
        ballTransform.Position.y = math.max(ballTransform.Position.y, minY);
        
        return true;
    }

    private static void ResolveBallCollision(ref PhysicsVelocity ballVelocity, ref LocalTransform ballTransform, 
        in ColliderCastHit closestHit, float deltaTime)
    {
        var castStart = ballTransform.Position - ballVelocity.Linear * deltaTime;
        var castEnd = ballTransform.Position;
        ballTransform.Position = math.lerp(castStart, castEnd, closestHit.Fraction) + closestHit.SurfaceNormal * 0.01f;
        
        var sign = math.sign(closestHit.SurfaceNormal);

        if (sign.x != 0)
            ballVelocity.Linear.x = sign.x * math.abs(ballVelocity.Linear.x);

        if (sign.y != 0)
            ballVelocity.Linear.y = sign.y * math.abs(ballVelocity.Linear.y);
    }
}
