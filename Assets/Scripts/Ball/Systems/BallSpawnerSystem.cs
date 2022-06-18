using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[UpdateAfter(typeof(PaddleSpawnerSystem))]
public partial class BallSpawnerSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem _beginSimulationEcbSystem;
    
    protected override void OnCreate()
    {
        base.OnCreate();
        
        _beginSimulationEcbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
        
        RequireSingletonForUpdate<BallSpawnRequest>();
    }

    protected override void OnUpdate()
    {
        var prefabs = GetSingleton<ScenePrefabs>();
        var gameSettings = GetSingleton<GameSettings>();

        var ecb = _beginSimulationEcbSystem.CreateCommandBuffer();

        Entities
            .ForEach((in BallSpawnRequest spawnRequest) =>
            {
                var ball = ecb.Instantiate(prefabs.BallEntityPrefab);
                ecb.SetName(ball, "Ball");
                
                ecb.AddComponent(ball, new BallData { OwnerPaddle = spawnRequest.OwnerPaddle });
                ecb.AddComponent(ball, new OwnerPlayerId { Value = spawnRequest.OwnerPlayer });
                ecb.SetComponent(ball, new Translation { Value = spawnRequest.Position });

                if (spawnRequest.StuckToPaddle)
                    ecb.AddComponent(ball, new BallStuckToPaddle { StuckTime = gameSettings.BallMovingDelay });
                else
                    ecb.AddComponent(ball, new PhysicsVelocity { Linear = spawnRequest.Velocity, Angular = float3.zero });
                
                ecb.AddComponent(ball, new MaterialColorData { Value = new float4( 1, 1, 1, 1) });
                ecb.AddComponent(ball, new MaterialTextureSTData());
                
                var playerIndex = GetComponent<PlayerIndex>(spawnRequest.OwnerPlayer);
                ecb.AddComponent(ball, new TextureAnimationData { FrameIndex = playerIndex.Value });
                
                ecb.AppendToBuffer(spawnRequest.OwnerPaddle, new BallLink { Ball = ball });

                ecb.AddBuffer<SingleFrameComponent>(ball);
            }).Schedule();
        
        _beginSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }
}