using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup))]
public partial struct PaddleSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameSettings>();
        state.RequireForUpdate<ScenePrefabs>();
        state.RequireForUpdate<LevelsSettings>();
        state.RequireForUpdate<GameData>();
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<PaddleSpawnRequest>();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var levelsSettings = SystemAPI.GetSingleton<LevelsSettings>();
        var gameData = SystemAPI.GetSingleton<GameData>();
        
        var ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        
        new PaddleSpawnerJob
        {
            Ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged),
            Prefabs = SystemAPI.GetSingleton<ScenePrefabs>(),
            GameSettings = SystemAPI.GetSingleton<GameSettings>(),
            PlayerIndexLookup = SystemAPI.GetComponentLookup<PlayerIndex>(true),
            GameAreaWidth = levelsSettings.GameAreaWidth,
            BallSpeed = gameData.BallSpeed,
            Random = Random.CreateFromIndex(state.GlobalSystemVersion)
        }.Schedule();
    }
    
    [BurstCompile]
    public partial struct PaddleSpawnerJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;
        public ScenePrefabs Prefabs;
        public GameSettings GameSettings;
        public int GameAreaWidth;
        public float BallSpeed;
        public Random Random;
        
        [ReadOnly] public ComponentLookup<PlayerIndex> PlayerIndexLookup;

        private void Execute(in PaddleSpawnRequest spawner)
        {
            var playerIndex = PlayerIndexLookup[spawner.OwnerPlayer];
            
            var paddle = Ecb.Instantiate(Prefabs.PaddleEntityPrefab);
            Ecb.SetName(paddle, "Paddle");

            //TODO: move to settings
            var position = new float3((playerIndex.Value + 1) % 2 == 0 ? GameAreaWidth - 6 : 6.0f, 
                playerIndex.Value < 2 ? 1.0f : 2.0f, 0.0f);
            
            Ecb.AddComponent(paddle, LocalTransform.FromPosition(position));
            Ecb.AddComponent(paddle, new PaddleData { Size = GameSettings.PaddleSize, Speed = GameSettings.PaddleSpeed});
            Ecb.AddComponent(paddle, new PaddleInputData());
            Ecb.AddComponent(paddle, new OwnerPlayerId { Value = spawner.OwnerPlayer });
            Ecb.AddComponent(paddle, new PlayerIndex { Value = playerIndex.Value });
            Ecb.AddComponent(paddle, new MaterialColorData { Value = new float4( 1, 1, 1, 1) });

            Ecb.AddComponent(paddle, new MaterialTextureSTData());
            Ecb.AddComponent(paddle, new TextureAnimationData());
            Ecb.AddComponent(paddle, new PlayTextureAnimation
            {
                FrameTime = 0.15f, Type = TextureAnimationType.PingPong, 
                StartFrame = 4 * playerIndex.Value, FramesCount = 4
            });
            
            Ecb.AddBuffer<BallLink>(paddle);
            
            SpawnBallOnPaddle(Ecb, paddle, position, spawner.OwnerPlayer, true, BallSpeed, Random);
            
            Ecb.AddBuffer<SingleFrameComponent>(paddle);
            
            Ecb.AddComponent(paddle, new HitByBallEvent());
            Ecb.SetComponentEnabled<HitByBallEvent>(paddle, false);
            
            Ecb.AddComponent(paddle, new BallLostEvent());
            Ecb.SetComponentEnabled<BallLostEvent>(paddle, false);
            
            Ecb.AddComponent(paddle, new PowerUpReceivedEvent());
            Ecb.SetComponentEnabled<PowerUpReceivedEvent>(paddle, false);
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
}