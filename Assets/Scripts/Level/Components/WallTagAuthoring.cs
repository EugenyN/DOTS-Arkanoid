using Unity.Entities;
using UnityEngine;

public class WallTagAuthoring : MonoBehaviour
{
    public class Baker : Baker<WallTagAuthoring>
    {
        public override void Bake(WallTagAuthoring authoring)
        {
            AddComponent(new WallTag());
        }
    }
}