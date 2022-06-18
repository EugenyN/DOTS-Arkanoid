using Unity.Entities;

public struct HiScoreUpdatedEvent : IComponentData
{
    public int Score;
}