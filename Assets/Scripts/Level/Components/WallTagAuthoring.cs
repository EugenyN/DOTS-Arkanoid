using Unity.Entities;
using UnityEngine;

public class WallTagAuthoring : MonoBehaviour
{
    public class Baker : Baker<WallTagAuthoring>
    {
        public override void Bake(WallTagAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new WallTag());
            AddComponent(entity, new HitByBallEvent());
            SetComponentEnabled<HitByBallEvent>(entity, false);
            AddComponent(entity, new HitByLaserEvent());
            SetComponentEnabled<HitByLaserEvent>(entity, false);
        }
    }
}