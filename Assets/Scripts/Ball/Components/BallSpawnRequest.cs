using Unity.Entities;
using Unity.Mathematics;

public struct BallSpawnRequest : IComponentData
{
    public float3 Position;
    public float3 Velocity;
    public Entity OwnerPaddle;
    public Entity OwnerPlayer;
    public bool StuckToPaddle;
}