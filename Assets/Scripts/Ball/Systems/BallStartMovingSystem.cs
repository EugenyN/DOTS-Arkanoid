using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

public partial class BallStartMovingSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

    protected override void OnCreate()
    {
        _endSimulationEcbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();

        var gameData = GetSingleton<GameData>();
        var randomSeed = (uint)System.Environment.TickCount;
        
        Entities
            .WithAll<BallStartMovingTag, BallStuckToPaddle>()
            .ForEach((Entity ball, in BallData ballData, in Translation position) =>
            {
                ecb.RemoveComponent<BallStuckToPaddle>(ball);
                ecb.RemoveComponent<BallStartMovingTag>(ball);

                var paddlePosition = GetComponent<Translation>(ballData.OwnerPaddle);
                var paddleData = GetComponent<PaddleData>(ballData.OwnerPaddle);
                
                var direction =
                    BallsHelper.GetBounceDirection(position.Value, paddlePosition.Value, paddleData.Size);

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