using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup))]
[UpdateAfter(typeof(PaddleSpawnerSystem))]
public partial class BallSpawnerSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem _beginSimulationEcbSystem;
    
    protected override void OnCreate()
    {
        base.OnCreate();
        
        _beginSimulationEcbSystem = World.GetExistingSystemManaged<BeginSimulationEntityCommandBufferSystem>();
        
        RequireForUpdate<BallSpawnRequest>();
    }

    protected override void OnUpdate()
    {
        var prefabs = SystemAPI.GetSingleton<ScenePrefabs>();
        var gameSettings = SystemAPI.GetSingleton<GameSettings>();

        var ecb = _beginSimulationEcbSystem.CreateCommandBuffer();

        Entities
            .ForEach((in BallSpawnRequest spawnRequest) =>
            {
                var ball = ecb.Instantiate(prefabs.BallEntityPrefab);
                ecb.SetName(ball, "Ball");
                
                ecb.AddComponent(ball, new BallData { OwnerPaddle = spawnRequest.OwnerPaddle });
                ecb.AddComponent(ball, new OwnerPlayerId { Value = spawnRequest.OwnerPlayer });
                ecb.SetComponent(ball, LocalTransform.FromPosition(spawnRequest.Position));

                if (spawnRequest.StuckToPaddle)
                    ecb.AddComponent(ball, new BallStuckToPaddle { StuckTime = gameSettings.BallMovingDelay });
                else
                    ecb.AddComponent(ball, new PhysicsVelocity { Linear = spawnRequest.Velocity, Angular = float3.zero });
                
                ecb.AddComponent(ball, new MaterialColorData { Value = new float4( 1, 1, 1, 1) });
                ecb.AddComponent(ball, new MaterialTextureSTData());
                
                var playerIndex = SystemAPI.GetComponent<PlayerIndex>(spawnRequest.OwnerPlayer);
                ecb.AddComponent(ball, new TextureAnimationData { FrameIndex = playerIndex.Value });
                
                ecb.AppendToBuffer(spawnRequest.OwnerPaddle, new BallLink { Ball = ball });

                ecb.AddBuffer<SingleFrameComponent>(ball);
            }).Schedule();
        
        _beginSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }
}