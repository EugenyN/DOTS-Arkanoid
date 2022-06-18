using Unity.Entities;
using Unity.Mathematics;

public struct LaserSpawnRequest : IComponentData
{
    public float3 Position;
    public Entity OwnerPlayer;
}