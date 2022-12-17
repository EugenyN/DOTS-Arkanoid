using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
public partial class BallCollisionResolvingSystem : SystemBase
{
    private EndFixedStepSimulationEntityCommandBufferSystem _endFixedStepSimulationEcbSystem;

    protected override void OnCreate()
    {
        _endFixedStepSimulationEcbSystem = 
            World.GetExistingSystemManaged<EndFixedStepSimulationEntityCommandBufferSystem>();
    }

    protected override unsafe void OnUpdate()
    {
        var ecb = _endFixedStepSimulationEcbSystem.CreateCommandBuffer();

        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        var colliderCastHits = new NativeList<ColliderCastHit>(Allocator.TempJob);
        
        Entities
            .WithAny<BallData>()
            .WithReadOnly(physicsWorld)
            .WithDisposeOnCompletion(colliderCastHits)
            .ForEach(
                (Entity entity, ref PhysicsVelocity velocity, in LocalTransform transform, in PhysicsCollider collider) =>
                {
                    colliderCastHits.Clear();

                    var colliderCastInput = new ColliderCastInput
                    {
                        Collider = (Collider*)collider.Value.GetUnsafePtr(), Start = transform.Position,
                        End = transform.Position
                    };
                    
                    physicsWorld.CastCollider(colliderCastInput, ref colliderCastHits);

                    if (colliderCastHits.IsCreated && colliderCastHits.Length != 0)
                    {
                        float distToHitEntity = float.MaxValue;
                        ColliderCastHit closestHit = default;

                        for (int i = 0; i < colliderCastHits.Length; i++)
                        {
                            var hit = colliderCastHits[i];

                            //PERF: structural changes
                            ecb.AddSingleFrameComponent(hit.Entity, new HitByBallEvent { Ball = entity });
                            ecb.AddSingleFrameComponent(entity, new BallHitEvent { HitEntity = hit.Entity });
                            
                            var hitEntityTransform = SystemAPI.GetComponent<LocalTransform>(hit.Entity);
                            var dist = math.distancesq(hitEntityTransform.Position, transform.Position);
                            if (dist < distToHitEntity)
                            {
                                distToHitEntity = dist;
                                closestHit = hit;
                            }
                        }
                        
                        if (SystemAPI.HasComponent<PaddleData>(closestHit.Entity))
                        {
                            var paddleData = SystemAPI.GetComponent<PaddleData>(closestHit.Entity);
                            var hitEntityTransform = SystemAPI.GetComponent<LocalTransform>(closestHit.Entity);
                            ResolvePaddleCollision(ref velocity, transform.Position, hitEntityTransform.Position,
                                paddleData.Size);
                        }
                        else if (SystemAPI.HasComponent<WallTag>(closestHit.Entity))
                        {
                            ResolveBallCollision(ref velocity, closestHit.SurfaceNormal);
                        }
                        else if (SystemAPI.HasComponent<BlockData>(closestHit.Entity))
                        {
                            if (SystemAPI.HasComponent<MegaBallTag>(entity) && !SystemAPI.HasComponent<GoldBlock>(closestHit.Entity))
                            {
                                // ignore
                            }
                            else
                            {
                                ResolveBallCollision(ref velocity, closestHit.SurfaceNormal);
                            }
                        }
                    }
                }).Schedule();

        _endFixedStepSimulationEcbSystem.AddJobHandleForProducer(Dependency);
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