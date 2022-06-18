using Unity.Entities;

public struct SingleFrameComponent : IBufferElementData
{
    public ComponentType TargetComponent;
}