using Unity.Collections;
using Unity.Entities;

public partial class PowerUpReceivingSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
    
    protected override void OnCreate()
    {
        base.OnCreate();
        
        _endSimulationEcbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        
        RequireForUpdate( GetEntityQuery(new EntityQueryDesc {
            All = new ComponentType[] { typeof(PowerUpReceivedEvent), typeof(PaddleData) }
        }));
    }
    
    protected override void OnUpdate()
    {
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();

        var gameSettings = GetSingleton<GameSettings>();
        
        Entities
            .ForEach((Entity paddle, ref PaddleData paddleData, in
                PowerUpReceivedEvent powerUpReceivedEvent, in OwnerPlayerId ownerPlayerId) =>
            {
                var playerData = GetComponent<PlayerData>(ownerPlayerId.Value);
                playerData.Score += powerUpReceivedEvent.Type == PowerUpType.Break ?
                    gameSettings.BreakPowerUpScore : gameSettings.PowerUpScore;
                
                SetComponent(ownerPlayerId.Value, playerData);
                
                if (PowerUpsHelper.IsExclusivePowerUp(powerUpReceivedEvent.Type))
                    paddleData.ExclusivePowerUp = powerUpReceivedEvent.Type;
                
                ecb.DestroyEntity(powerUpReceivedEvent.PowerUp);
                
                AudioSystem.PlayAudio(ecb, AudioClipKeys.PowerUpReceived);
            }).Schedule();
        
        _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }
}