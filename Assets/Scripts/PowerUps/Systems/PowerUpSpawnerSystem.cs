using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

[UpdateInGroup(typeof(PowerUpsSystemGroup))]
public partial struct PowerUpSpawnerSystem : ISystem
{
    private const float DropSpeed = -10; // move to settings
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ScenePrefabs>();
        state.RequireForUpdate<GameSettings>();
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

        var gameSettings = SystemAPI.GetSingleton<GameSettings>();
        var prefabs = SystemAPI.GetSingleton<ScenePrefabs>();
        
        new PowerUpSpawnerJob
        {
            Ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged),
            PowerUpEntityPrefab = prefabs.PowerUpEntityPrefab,
            PowerUpProbability = gameSettings.PowerUpProbability,
            Random = Random.CreateFromIndex(state.GlobalSystemVersion)
        }.Schedule();
    }
    
    [BurstCompile]
    [WithAll(typeof(HitByBallEvent), typeof(BlockData))]
    [WithNone(typeof(PowerUpData))]
    public partial struct PowerUpSpawnerJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;
        public Entity PowerUpEntityPrefab;
        public float PowerUpProbability;
        public Random Random;
        
        private void Execute(in LocalTransform transform)
        {
            if (Random.NextFloat() < PowerUpProbability)
            {
                var type = (PowerUpType)Random.NextInt((int)PowerUpType.PowerUpsCount);
                SpawnPowerUp(Ecb, PowerUpEntityPrefab, type, transform);
            }
        }
        
        private static void SpawnPowerUp(EntityCommandBuffer ecb, Entity prefab, PowerUpType powerUpType,
            in LocalTransform transform)
        {
            var entity = ecb.Instantiate(prefab);
            ecb.SetName(entity, "PowerUp");

            ecb.AddComponent(entity, transform);
            ecb.AddComponent(entity, new PowerUpData { Type = powerUpType });
            ecb.AddComponent(entity, new MaterialTextureSTData());
            ecb.AddComponent(entity, new TextureAnimationData());
            ecb.AddComponent(entity, new PlayTextureAnimation
            {
                FrameTime = 0.1f, Type = TextureAnimationType.Loop, StartFrame = 8 * (int)powerUpType, FramesCount = 8
            });

            ecb.AddComponent(entity, new PhysicsVelocity { Linear = new float3(0, DropSpeed, 0), Angular = float3.zero });
        }
    }
}