using Unity.Entities;

public struct ChangeStateCommand : IComponentData
{
    public ComponentType TargetState;
}