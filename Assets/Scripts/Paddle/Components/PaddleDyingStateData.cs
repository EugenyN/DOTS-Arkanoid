using Unity.Entities;

public enum PaddleDyingState
{
    Dying,
    DyingComplete,
    RespawnOrGameOver
}

public struct PaddleDyingStateData : IComponentData
{
    public PaddleDyingState State;
    public float StateTimer;
}