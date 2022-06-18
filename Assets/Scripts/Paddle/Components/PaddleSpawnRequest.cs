using Unity.Entities;

public struct PaddleSpawnRequest : IComponentData
{
    public Entity OwnerPlayer;
}