using Unity.Entities;

public struct HitByLaserEvent : IComponentData
{
    public Entity LaserShot;
}