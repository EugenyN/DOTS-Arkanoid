using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup))]
public partial class BlockHitSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

    protected override void OnCreate()
    {
        _endSimulationEcbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();
        
        var damagedByEntity = new NativeList<Entity>(Allocator.TempJob);

        var ballHitJobHandle = Entities
            .ForEach((ref BlockData blockData, in HitByBallEvent hitByBall) =>
        {
            if (blockData.Type != BlockTypes.Gold)
            {
                if (SystemAPI.HasComponent<MegaBallTag>(hitByBall.Ball))
                    blockData.Health = 0;
                else
                    blockData.Health--;
            }
            
            damagedByEntity.Add(hitByBall.Ball);
            AudioSystem.PlayAudio(ecb, blockData.Health <= 0 ? AudioClipKeys.BlockDestroy : AudioClipKeys.BlockDamage);
        }).Schedule(Dependency);

        var laserHitJobHandle = Entities.ForEach((ref BlockData blockData, in HitByLaserEvent hitByLaser) =>
        {
            if (blockData.Type != BlockTypes.Gold)
                blockData.Health--;

            damagedByEntity.Add(hitByLaser.LaserShot);
        }).Schedule(ballHitJobHandle);

        var dependencies = JobHandle.CombineDependencies(ballHitJobHandle, laserHitJobHandle);
        
        Dependency = Entities
            .WithAny<HitByBallEvent, HitByLaserEvent>()
            .WithDisposeOnCompletion(damagedByEntity)
            .ForEach((Entity block, in BlockData blockData) =>
        {
            if (damagedByEntity.Length != 0)
            {
                if (blockData.Health > 0)
                {
                    int frameIndex = blockData.Type switch
                    {
                        BlockTypes.Silver => 2 * 6, BlockTypes.Gold => 3 * 6, _ => 0
                    };
                    ecb.AddComponent(block, new PlayTextureAnimation
                    {
                        StartFrame = frameIndex, FramesCount = 6, FrameTime = 0.05f,
                        Type = TextureAnimationType.Once
                    });
                }
                else
                {
                    var playerEntity = SystemAPI.GetComponent<OwnerPlayerId>(damagedByEntity[0]);
                    var playerData = SystemAPI.GetComponent<PlayerData>(playerEntity.Value);
                    playerData.Score += 50 + (int)blockData.Type * 10;
                    ecb.SetComponent(playerEntity.Value, playerData);

                    ecb.DestroyEntity(block);
                }
            }
        }).Schedule(dependencies);
        
        _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }
}