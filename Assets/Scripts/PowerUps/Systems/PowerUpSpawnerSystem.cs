using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

public partial class PowerUpSpawnerSystem : SystemBase
{
    private EntityQuery _powerUpsQuery;
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
    
    private const float DropSpeed = -10;

    protected override void OnCreate()
    {
        base.OnCreate();
        
        _powerUpsQuery = GetEntityQuery(typeof(PowerUpData));
        _endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        
        RequireSingletonForUpdate<HitByBallEvent>();
    }

    protected override void OnUpdate()
    {
        if (!_powerUpsQuery.IsEmpty)
            return;

        var gameSettings = GetSingleton<GameSettings>();
        var prefabs = GetSingleton<ScenePrefabs>();
        var powerUpsCount = Enum.GetValues(typeof(PowerUpType)).Length;

        var random = new Random((uint)Environment.TickCount);
        
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();

        Entities.WithAll<HitByBallEvent, BlockData>().ForEach((in Translation position) =>
        {
            if (random.NextFloat() < gameSettings.PowerUpProbability)
            {
                var type = (PowerUpType)random.NextInt(powerUpsCount);
                SpawnPowerUp(ecb, prefabs.PowerUpEntityPrefab, type, position);
            }
        }).Schedule();
        
        _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }

    private static void SpawnPowerUp(EntityCommandBuffer ecb, Entity prefab, PowerUpType powerUpType,
        Translation position)
    {
        var entity = ecb.Instantiate(prefab);
        ecb.SetName(entity, "PowerUp");

        ecb.AddComponent(entity, position);
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