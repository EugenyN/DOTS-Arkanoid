using Unity.Entities;

public struct PowerUpReceivedEvent : IComponentData, IEnableableComponent
{
    public Entity PowerUp;
    public PowerUpType Type;
}