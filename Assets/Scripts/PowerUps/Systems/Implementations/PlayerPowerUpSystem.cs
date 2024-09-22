using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(PowerUpsSystemGroup))]
public partial struct PlayerPowerUpSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameSettings>();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var gameSettings = SystemAPI.GetSingleton<GameSettings>();
        
        new LaserPowerUpJob
        {
            PlayerDataLookup = SystemAPI.GetComponentLookup<PlayerData>(),
            PlayerMaxLives = gameSettings.PlayerMaxLives
        }.Schedule();
    }
    
    [BurstCompile]
    public partial struct LaserPowerUpJob : IJobEntity
    {
        public ComponentLookup<PlayerData> PlayerDataLookup;
        public int PlayerMaxLives;
        
        private void Execute(in PowerUpReceivedEvent request, in OwnerPlayerId ownerPlayerId)
        {
            if (request.Type == PowerUpType.Player)
            {
                var playerData = PlayerDataLookup[ownerPlayerId.Value];
                playerData.Lives++;
                playerData.Lives = math.min(playerData.Lives, PlayerMaxLives);
                PlayerDataLookup[ownerPlayerId.Value] = playerData;
            }
        }
    }
}