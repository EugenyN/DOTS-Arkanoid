using Unity.Entities;

[GenerateAuthoringComponent]
public struct TextureSheetConfig : IComponentData
{
    public int FrameColumns;
    public int FrameRows;
}