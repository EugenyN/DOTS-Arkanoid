using Unity.Entities;

public enum TextureAnimationType
{
    Once,
    Loop,
    PingPong
}

public struct PlayTextureAnimation : IComponentData
{
    public TextureAnimationType Type;
    public float FrameTime;
    public int StartFrame;
    public int FramesCount;
    public bool Initialized;
}