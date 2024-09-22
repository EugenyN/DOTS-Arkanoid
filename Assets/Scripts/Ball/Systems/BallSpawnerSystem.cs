using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup))]
[UpdateAfter(typeof(PaddleSpawnerSystem))]
public partial struct BallSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameSettings>();
        state.RequireForUpdate<ScenePrefabs>();
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<BallSpawnRequest>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        
        new BallSpawnerJob
        {
            Ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged),
            Prefabs = SystemAPI.GetSingleton<ScenePrefabs>(),
            GameSettings = SystemAPI.GetSingleton<GameSettings>(),
            PlayerIndexLookup = SystemAPI.GetComponentLookup<PlayerIndex>(true)
        }.Schedule();
    }

    [BurstCompile]
    public partial struct BallSpawnerJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;
        public ScenePrefabs Prefabs;
        public GameSettings GameSettings;
        
        [ReadOnly] public ComponentLookup<PlayerIndex> PlayerIndexLookup;

        private void Execute(in BallSpawnRequest spawnRequest)
        {
            var ball = Ecb.Instantiate(Prefabs.BallEntityPrefab);
            Ecb.SetName(ball, "Ball");

            Ecb.AddComponent(ball, new BallData { OwnerPaddle = spawnRequest.OwnerPaddle });
            Ecb.AddComponent(ball, new OwnerPlayerId { Value = spawnRequest.OwnerPlayer });
            Ecb.SetComponent(ball, LocalTransform.FromPosition(spawnRequest.Position));

            if (spawnRequest.StuckToPaddle)
                Ecb.AddComponent(ball, new BallStuckToPaddle { StuckTime = GameSettings.BallMovingDelay });
            else
                Ecb.AddComponent(ball, new PhysicsVelocity { Linear = spawnRequest.Velocity, Angular = float3.zero });

            Ecb.AddComponent(ball, new MaterialColorData { Value = new float4(1, 1, 1, 1) });
            Ecb.AddComponent(ball, new MaterialTextureSTData());

            var playerIndex = PlayerIndexLookup[spawnRequest.OwnerPlayer];
            Ecb.AddComponent(ball, new TextureAnimationData { FrameIndex = playerIndex.Value });

            Ecb.AppendToBuffer(spawnRequest.OwnerPaddle, new BallLink { Ball = ball });

            Ecb.AddComponent(ball, new MaterialTextureSTData());
            
            Ecb.AddBuffer<BallHitEvent>(ball);
        }
    }
}