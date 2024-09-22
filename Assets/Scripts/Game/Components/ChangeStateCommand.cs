using Unity.Entities;

public struct ChangeStateCommand : IComponentData
{
    public ComponentType TargetState;

    public static ChangeStateCommand Create<T>() => new() { TargetState = ComponentType.ReadWrite<T>() };
}