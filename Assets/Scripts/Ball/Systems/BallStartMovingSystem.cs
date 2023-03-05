using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup))]
public partial class BallStartMovingSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

    protected override void OnCreate()
    {
        _endSimulationEcbSystem = World.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
        
        RequireForUpdate<GameData>();
    }

    protected override void OnUpdate()
    {
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();

        var gameData = SystemAPI.GetSingleton<GameData>();
        var randomSeed = (uint)System.Environment.TickCount;
        
        Entities
            .WithAll<BallStartMovingTag, BallStuckToPaddle>()
            .ForEach((Entity ball, in BallData ballData, in LocalTransform transform) =>
            {
                ecb.RemoveComponent<BallStuckToPaddle>(ball);
                ecb.RemoveComponent<BallStartMovingTag>(ball);

                var paddleTransform = SystemAPI.GetComponent<LocalTransform>(ballData.OwnerPaddle);
                var paddleData = SystemAPI.GetComponent<PaddleData>(ballData.OwnerPaddle);
                
                var direction =
                    BallsHelper.GetBounceDirection(transform.Position, paddleTransform.Position, paddleData.Size);

                if (direction.Equals(math.up()))
                    direction = BallsHelper.GetRandomDirection(new Random(randomSeed));

                ecb.AddComponent(ball, new PhysicsVelocity
                {
                    Linear = direction * gameData.BallSpeed, Angular = float3.zero
                });

                AudioSystem.PlayAudio(ecb, AudioClipKeys.PaddleHit);
            }).Schedule();

        _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }
}