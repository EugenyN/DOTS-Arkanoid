using Unity.Entities;

public struct HitByBallEvent : IComponentData, IEnableableComponent
{
    public Entity Ball;
}