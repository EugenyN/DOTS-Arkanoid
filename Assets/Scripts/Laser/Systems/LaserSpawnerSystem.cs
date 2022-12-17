using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

public partial class LaserSpawnerSystem : SystemBase
{
    private const float LaserSpeed = 20;
    
    private BeginSimulationEntityCommandBufferSystem _beginSimulationEcbSystem;

    protected override void OnCreate()
    {
        base.OnCreate();
        
        _beginSimulationEcbSystem = World.GetExistingSystemManaged<BeginSimulationEntityCommandBufferSystem>();
        
        RequireForUpdate<LaserSpawnRequest>();
    }
    
    protected override void OnUpdate()
    {
        var prefabs = SystemAPI.GetSingleton<ScenePrefabs>();

        var ecb = _beginSimulationEcbSystem.CreateCommandBuffer();

        Entities
            .ForEach((in LaserSpawnRequest spawnRequest) =>
            {
                SpawnLaser(ecb, prefabs.LaserEntityPrefab, spawnRequest.Position, spawnRequest.OwnerPlayer);
            }).Schedule();
        
        _beginSimulationEcbSystem.AddJobHandleForProducer(Dependency);
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
    
    public void SpawnLaserShot(EntityCommandBuffer ecb, Entity paddle, Entity player)
    {
        var paddleTransform = SystemAPI.GetComponent<LocalTransform>(paddle);
        
        ecb.AddSingleFrameComponent(new LaserSpawnRequest { 
            Position = paddleTransform.Position + new float3(0, 1, 0), OwnerPlayer = player
        });
        
        AudioSystem.PlayAudio(ecb, AudioClipKeys.LaserShot);
    }
}