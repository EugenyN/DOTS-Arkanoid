using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup))]
public partial struct LaserSpawnerSystem : ISystem
{
    private const float LaserSpeed = 20;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<ScenePrefabs>();
        state.RequireForUpdate<LaserSpawnRequest>();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var prefabs = SystemAPI.GetSingleton<ScenePrefabs>();
        var ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        
        new LaserSpawnerJob
        {
            Ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged),
            LaserEntityPrefab = prefabs.LaserEntityPrefab
        }.Schedule();
    }
    
    [BurstCompile]
    public partial struct LaserSpawnerJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;
        public Entity LaserEntityPrefab;
        
        private void Execute(in LaserSpawnRequest spawnRequest)
        {
            SpawnLaser(Ecb, LaserEntityPrefab, spawnRequest.Position, spawnRequest.OwnerPlayer);
        }
        
        private static void SpawnLaser(EntityCommandBuffer ecb, Entity prefab, float3 position, Entity ownerPlayerId)
        {
            var laserShot = ecb.Instantiate(prefab);
            ecb.SetName(laserShot, "LaserShot");

            ecb.AddComponent(laserShot, LocalTransform.FromPosition(position));
            ecb.AddComponent(laserShot, new LaserShotTag());
            ecb.AddComponent(laserShot, new PhysicsVelocity { Linear = new float3(0, LaserSpeed, 0), Angular = float3.zero });
            ecb.AddComponent(laserShot, new OwnerPlayerId { Value = ownerPlayerId });
        }
    }
}