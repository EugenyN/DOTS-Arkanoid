using Unity.Entities;

public struct GamePausedEvent : IComponentData
{
    public bool Paused;
}