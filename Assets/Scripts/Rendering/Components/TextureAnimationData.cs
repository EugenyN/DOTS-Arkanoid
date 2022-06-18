using Unity.Entities;

public struct TextureAnimationData : IComponentData
{
    public float Time;
    public int FrameIndex;
    public bool IndexDecrement;
}