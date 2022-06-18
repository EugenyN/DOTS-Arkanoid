using Unity.Entities;

public struct PowerUpReceivedEvent : IComponentData
{
    public Entity PowerUp;
    public PowerUpType Type;
}