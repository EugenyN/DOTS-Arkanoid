using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup))]
public partial struct BlockHitSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<PlayerData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);

        var damagedByEntity = new NativeList<Entity>(Allocator.TempJob);

        var ballHitJobHandle = new BallHitJob
        {
            Ecb = ecb,
            DamagedByEntity = damagedByEntity,
            MegaBallTagLookup = SystemAPI.GetComponentLookup<MegaBallTag>(true)
        }.Schedule(state.Dependency);

        var laserHitJobHandle = new LaserHitJob
        {
            DamagedByEntity = damagedByEntity,
        }.Schedule(ballHitJobHandle);

        var dependencies = JobHandle.CombineDependencies(ballHitJobHandle, laserHitJobHandle);

        state.Dependency = new BlockHitJob
        {
            Ecb = ecb,
            DamagedByEntity = damagedByEntity,
            OwnerPlayerIdLookup = SystemAPI.GetComponentLookup<OwnerPlayerId>(true),
            PlayerDataLookup = SystemAPI.GetComponentLookup<PlayerData>(true)
        }.Schedule(dependencies);

        damagedByEntity.Dispose(state.Dependency);
    }

    [BurstCompile]
    public partial struct BallHitJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;
        public NativeList<Entity> DamagedByEntity;
        [ReadOnly] public ComponentLookup<MegaBallTag> MegaBallTagLookup;

        private void Execute(ref BlockData blockData, in HitByBallEvent hitByBallEvent)
        {
            if (blockData.Type != BlockTypes.Gold)
            {
                if (MegaBallTagLookup.HasComponent(hitByBallEvent.Ball))
                    blockData.Health = 0;
                else
                    blockData.Health--;
            }

            DamagedByEntity.Add(hitByBallEvent.Ball);
            
            AudioSystem.PlayAudio(Ecb, blockData.Health <= 0 ? AudioClipKeys.BlockDestroy : AudioClipKeys.BlockDamage);
        }
    }

    [BurstCompile]
    public partial struct LaserHitJob : IJobEntity
    {
        public NativeList<Entity> DamagedByEntity;

        private void Execute(ref BlockData blockData, HitByLaserEvent hitByLaserEvent)
        {
            if (blockData.Type != BlockTypes.Gold)
                blockData.Health--;

            DamagedByEntity.Add(hitByLaserEvent.LaserShot);
        }
    }

    [BurstCompile]
    [WithAny(typeof(HitByBallEvent), typeof(HitByLaserEvent))]
    public partial struct BlockHitJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;
        [ReadOnly] public NativeList<Entity> DamagedByEntity;
        [ReadOnly] public ComponentLookup<OwnerPlayerId> OwnerPlayerIdLookup;
        [ReadOnly] public ComponentLookup<PlayerData> PlayerDataLookup;

        private void Execute(Entity block, in BlockData blockData)
        {
            if (DamagedByEntity.Length == 0)
                return;

            if (blockData.Health > 0)
            {
                int frameIndex = blockData.Type switch
                {
                    BlockTypes.Silver => 2 * 6, BlockTypes.Gold => 3 * 6, _ => 0
                };
                Ecb.AddComponent(block, new PlayTextureAnimation
                {
                    StartFrame = frameIndex, FramesCount = 6, FrameTime = 0.05f,
                    Type = TextureAnimationType.Once
                });
            }
            else
            {
                var playerEntity = OwnerPlayerIdLookup[DamagedByEntity[0]];
                var playerData = PlayerDataLookup[playerEntity.Value];
                playerData.Score += 50 + (int)blockData.Type * 10;
                Ecb.SetComponent(playerEntity.Value, playerData);

                Ecb.DestroyEntity(block);
            }
        }
    }
}