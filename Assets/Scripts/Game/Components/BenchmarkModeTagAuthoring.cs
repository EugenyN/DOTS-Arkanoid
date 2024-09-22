using Unity.Entities;
using UnityEngine;

public class BenchmarkModeTagAuthoring : MonoBehaviour
{
    public class BenchmarkModeTagBaker : Baker<BenchmarkModeTagAuthoring>
    {
        public override void Bake(BenchmarkModeTagAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent<BenchmarkModeTag>(entity);
        }
    }
}