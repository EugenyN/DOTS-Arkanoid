using Unity.Entities;

public struct BlocksSpawnRequest : IComponentData
{
    public Entity BlockPrefab;
}