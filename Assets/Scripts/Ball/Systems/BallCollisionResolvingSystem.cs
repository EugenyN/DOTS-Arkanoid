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
            LocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true),
            PaddleDataLookup = SystemAPI.GetComponentLookup<PaddleData>(true),
            WallTagLookup = SystemAPI.GetComponentLookup<WallTag>(true),
            BlockDataLookup = SystemAPI.GetComponentLookup<BlockData>(true),
            MegaBallTagLookup = SystemAPI.GetComponentLookup<MegaBallTag>(true),
            GoldBlockLookup = SystemAPI.GetComponentLookup<GoldBlock>(true),
            HitByBallEventLookup = SystemAPI.GetComponentLookup<HitByBallEvent>()
        }.Schedule(state.Dependency);

        colliderCastHits.Dispose(state.Dependency);
    }

    [BurstCompile]
    [WithAny(typeof(BallData))]
    public partial struct BallCollisionResolvingJob : IJobEntity
    {
        public NativeList<ColliderCastHit> ColliderCastHits;
        
        [ReadOnly] public CollisionWorld World;
        [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
        [ReadOnly] public ComponentLookup<PaddleData> PaddleDataLookup;
        [ReadOnly] public ComponentLookup<WallTag> WallTagLookup;
        [ReadOnly] public ComponentLookup<BlockData> BlockDataLookup;
        [ReadOnly] public ComponentLookup<MegaBallTag> MegaBallTagLookup;
        [ReadOnly] public ComponentLookup<GoldBlock> GoldBlockLookup;
        
        public ComponentLookup<HitByBallEvent> HitByBallEventLookup;

        private void Execute(Entity ball, ref PhysicsVelocity velocity, in LocalTransform transform,
            in PhysicsCollider collider, ref DynamicBuffer<BallHitEvent> ballHitEvents)
        {
            ColliderCastHits.Clear();

            unsafe
            {
                var colliderCastInput = new ColliderCastInput {
                    Collider = collider.ColliderPtr, Start = transform.Position, End = transform.Position
                };

                World.CastCollider(colliderCastInput, ref ColliderCastHits);
            }

            if (ColliderCastHits.IsCreated && ColliderCastHits.Length != 0)
            {
                float distToHitEntity = float.MaxValue;
                ColliderCastHit closestHit = default;

                for (int i = 0; i < ColliderCastHits.Length; i++)
                {
                    var hit = ColliderCastHits[i];
                    
                    HitByBallEventLookup[hit.Entity] = new HitByBallEvent { Ball = ball };
                    HitByBallEventLookup.SetComponentEnabled(hit.Entity, true);
                    
                    ballHitEvents.Add(new BallHitEvent { HitEntity = hit.Entity });

                    var hitEntityTransform = LocalTransformLookup[hit.Entity];
                    var dist = math.distancesq(hitEntityTransform.Position, transform.Position);
                    if (dist < distToHitEntity)
                    {
                        distToHitEntity = dist;
                        closestHit = hit;
                    }
                }

                if (PaddleDataLookup.HasComponent(closestHit.Entity))
                {
                    var paddleData = PaddleDataLookup[closestHit.Entity];
                    var hitTransform = LocalTransformLookup[closestHit.Entity];
                    ResolvePaddleCollision(ref velocity, transform.Position, hitTransform.Position, paddleData.Size);
                }
                else if (WallTagLookup.HasComponent(closestHit.Entity))
                {
                    ResolveBallCollision(ref velocity, closestHit.SurfaceNormal);
                }
                else if (BlockDataLookup.HasComponent(closestHit.Entity))
                {
                    if (MegaBallTagLookup.HasComponent(ball) && !GoldBlockLookup.HasComponent(closestHit.Entity))
                    {
                        // ignore
                    }
                    else
                    {
                        ResolveBallCollision(ref velocity, closestHit.SurfaceNormal);
                    }
                }
            }
        }
    }

    private static bool ResolvePaddleCollision(ref PhysicsVelocity ballVelocity, float3 ballPosition,
        float3 paddlePosition, float3 paddleSize)
    {
        if (ballVelocity.Linear.y > 0)
            return false;

        if (ballPosition.y < paddlePosition.y - paddleSize.y / 2.0f)
            return false;

        var direction = BallsHelper.GetBounceDirection(ballPosition, paddlePosition, paddleSize);
        ballVelocity.Linear = direction * math.length(ballVelocity.Linear);
        return true;
    }

    private static void ResolveBallCollision(ref PhysicsVelocity ballVelocity, float3 normal)
    {
        var sign = math.sign(normal);

        if (sign.x != 0)
            ballVelocity.Linear.x = sign.x * math.abs(ballVelocity.Linear.x);

        if (sign.y != 0)
            ballVelocity.Linear.y = sign.y * math.abs(ballVelocity.Linear.y);
    }
}