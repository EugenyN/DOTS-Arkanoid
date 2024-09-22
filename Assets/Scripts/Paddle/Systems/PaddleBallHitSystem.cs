using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup))]
public partial struct PaddleBallHitSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<PaddleData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        
        new PaddleBallHitJob
        {
            Ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged),
            PlayerDataLookup = SystemAPI.GetComponentLookup<PlayerData>(),
            StickPaddleTagLookup = SystemAPI.GetComponentLookup<StickPaddleTag>()
        }.Schedule();
    }
    
    [BurstCompile]
    [WithAll(typeof(HitByBallEvent))]
    public partial struct PaddleBallHitJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;
        public ComponentLookup<PlayerData> PlayerDataLookup;
        [ReadOnly] public ComponentLookup<StickPaddleTag> StickPaddleTagLookup;
        
        private void Execute(Entity paddle, in OwnerPlayerId ownerPlayerId)
        {
            var playerData = PlayerDataLookup[ownerPlayerId.Value];
            playerData.Score += 10;
            PlayerDataLookup[ownerPlayerId.Value] = playerData;

            AudioSystem.PlayAudio(Ecb, StickPaddleTagLookup.HasComponent(paddle) ? 
                AudioClipKeys.PaddleCatch : AudioClipKeys.PaddleHit);
        }
    }
}