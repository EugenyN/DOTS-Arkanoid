using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(PowerUpsSystemGroup), OrderLast = true)]
[UpdateAfter(typeof(PowerUpTriggeringSystem))]
public partial struct PowerUpReceivingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameSettings>();
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var gameSettings = SystemAPI.GetSingleton<GameSettings>();
        var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

        new PowerUpReceivingJob
        {
            Ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged),
            PlayerDataLookup = SystemAPI.GetComponentLookup<PlayerData>(),
            PowerUpScore = gameSettings.PowerUpScore,
            BreakPowerUpScore = gameSettings.BreakPowerUpScore
        }.Schedule();
    }
    
    [BurstCompile]
    public partial struct PowerUpReceivingJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;
        public ComponentLookup<PlayerData> PlayerDataLookup;
        public int PowerUpScore;
        public int BreakPowerUpScore;
        
        private void Execute(ref PaddleData paddleData, ref PowerUpReceivedEvent powerUpReceivedEvent,
            EnabledRefRW<PowerUpReceivedEvent> powerUpReceivedEventEnabledRef, in OwnerPlayerId ownerPlayerId)
        {
            var playerData = PlayerDataLookup[ownerPlayerId.Value];
            
            playerData.Score += powerUpReceivedEvent.Type == PowerUpType.Break ? BreakPowerUpScore : PowerUpScore;
            
            PlayerDataLookup[ownerPlayerId.Value] = playerData;
                
            if (PowerUpsHelper.IsExclusivePowerUp(powerUpReceivedEvent.Type))
                paddleData.ExclusivePowerUp = powerUpReceivedEvent.Type;
                
            Ecb.DestroyEntity(powerUpReceivedEvent.PowerUp);

            powerUpReceivedEventEnabledRef.ValueRW = false;
                
            AudioSystem.PlayAudio(Ecb, AudioClipKeys.PowerUpReceived);
        }
    }
}