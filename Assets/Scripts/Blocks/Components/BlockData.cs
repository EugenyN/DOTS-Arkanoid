using Unity.Entities;

public enum BlockTypes
{
    None, White, Orange, LightBlue, Green, Red, Blue, Pink, Yellow, Silver, Gold
}

public struct BlockData : IComponentData
{
    public BlockTypes Type;
    public int Health;
}