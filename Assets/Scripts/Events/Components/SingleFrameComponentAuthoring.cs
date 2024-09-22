using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class SingleFrameComponentAuthoring : MonoBehaviour
{
    public class Baker : Baker<SingleFrameComponentAuthoring>
    {
        public override void Bake(SingleFrameComponentAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddBuffer<SingleFrameComponent>(entity);
        }
    }
}