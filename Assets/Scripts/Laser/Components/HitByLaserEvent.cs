using Unity.Entities;

public struct HitByLaserEvent : IComponentData, IEnableableComponent
{
    public Entity LaserShot;
}