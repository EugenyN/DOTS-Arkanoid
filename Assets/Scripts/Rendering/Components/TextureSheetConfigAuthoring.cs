using Unity.Entities;
using UnityEngine;

public class TextureSheetConfigAuthoring : MonoBehaviour
{
    public int FrameColumns;
    public int FrameRows;
    
    public class Baker : Baker<TextureSheetConfigAuthoring>
    {
        public override void Bake(TextureSheetConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new TextureSheetConfig
            {
                FrameColumns = authoring.FrameColumns,
                FrameRows = authoring.FrameRows
            });
        }
    }
}