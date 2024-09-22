using Unity.Entities;

public struct BallHitEvent : IBufferElementData
{
    public Entity HitEntity;
}