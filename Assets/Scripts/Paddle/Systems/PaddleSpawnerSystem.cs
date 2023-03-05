using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup))]
public partial class PaddleSpawnerSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem _beginSimulationEcbSystem;
    
    protected override void OnCreate()
    {
        base.OnCreate();
        
        _beginSimulationEcbSystem = World.GetExistingSystemManaged<BeginSimulationEntityCommandBufferSystem>();
        
        RequireForUpdate<PaddleSpawnRequest>();
    }
    
    protected override void OnUpdate()
    {
        var ecb = _beginSimulationEcbSystem.CreateCommandBuffer();
        
        var gameSettings = SystemAPI.GetSingleton<GameSettings>();
        var gameData = SystemAPI.GetSingleton<GameData>();
        var prefabs = SystemAPI.GetSingleton<ScenePrefabs>();
        
        var randomSeed = (uint)System.Environment.TickCount;
        
        Entities.ForEach((in PaddleSpawnRequest spawner) =>
        {
            var playerIndex = SystemAPI.GetComponent<PlayerIndex>(spawner.OwnerPlayer);
            
            var paddle = ecb.Instantiate(prefabs.PaddleEntityPrefab);
            ecb.SetName(paddle, "Paddle");

            //TODO: move to settings
            var position = new float3((playerIndex.Value + 1) % 2 == 0 ? GameConst.GameAreaWidth - 6 : 6.0f, 
                playerIndex.Value < 2 ? 1.0f : 2.0f, 0.0f);
            
            ecb.AddComponent(paddle, LocalTransform.FromPosition(position));
            ecb.AddComponent(paddle, new PaddleData { Size = gameSettings.PaddleSize, Speed = gameSettings.PaddleSpeed});
            ecb.AddComponent(paddle, new PaddleInputData());
            ecb.AddComponent(paddle, new OwnerPlayerId { Value = spawner.OwnerPlayer });
            ecb.AddComponent(paddle, new PlayerIndex { Value = playerIndex.Value });
            ecb.AddComponent(paddle, new MaterialColorData { Value = new float4( 1, 1, 1, 1) });

            ecb.AddComponent(paddle, new MaterialTextureSTData());
            ecb.AddComponent(paddle, new TextureAnimationData());
            ecb.AddComponent(paddle, new PlayTextureAnimation
            {
                FrameTime = 0.15f, Type = TextureAnimationType.PingPong, 
                StartFrame = 4 * playerIndex.Value, FramesCount = 4
            });
            
            ecb.AddBuffer<BallLink>(paddle);
            
            SpawnBallOnPaddle(ecb, paddle, position, spawner.OwnerPlayer, true, 
                gameData.BallSpeed, new Random(randomSeed));

            ecb.AddBuffer<SingleFrameComponent>(paddle);

        }).Schedule();
        
        _beginSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }
    
    private static void SpawnBallOnPaddle(EntityCommandBuffer ecb, Entity paddle, float3 paddlePosition, Entity player,
        bool stuckToPaddle, float ballSpeed, Random random)
    {
        ecb.AddSingleFrameComponent(new BallSpawnRequest
        {
            Position = paddlePosition + new float3(0, 1, 0),
            OwnerPaddle = paddle,
            OwnerPlayer = player,
            StuckToPaddle = stuckToPaddle,
            Velocity = BallsHelper.GetRandomDirection(random) * ballSpeed
        });
    }
}