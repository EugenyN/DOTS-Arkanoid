using Unity.Entities;

public struct GameStateData : IComponentData
{
    public float StateTimer;
    public ComponentType CurrentState;
}