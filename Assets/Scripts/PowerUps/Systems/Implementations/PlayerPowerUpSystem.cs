using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(PowerUpsSystemGroup))]
public partial class PlayerPowerUpSystem : SystemBase
{
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireForUpdate<PowerUpReceivedEvent>();
    }
    
    protected override void OnUpdate()
    {
        var gameSettings = SystemAPI.GetSingleton<GameSettings>();
        
        Entities
            .ForEach((in PowerUpReceivedEvent request, in OwnerPlayerId ownerPlayerId) =>
            {
                if (request.Type == PowerUpType.Player)
                {
                    var playerData = SystemAPI.GetComponent<PlayerData>(ownerPlayerId.Value);
                    playerData.Lives++;
                    playerData.Lives = math.min(playerData.Lives, gameSettings.PlayerMaxLives);
                    SystemAPI.SetComponent(ownerPlayerId.Value, playerData);
                }
            }).Schedule();
    }
}