using Unity.Entities;
using Unity.Mathematics;

public struct PaddleData : IComponentData
{
    public float3 Size;
    public float Speed;
    public PowerUpType ExclusivePowerUp;
}