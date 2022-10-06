using Unity.Entities;
using UnityEngine;

public class LevelTagAuthoring : MonoBehaviour
{
    public class Baker : Baker<LevelTagAuthoring>
    {
        public override void Bake(LevelTagAuthoring authoring)
        {
            AddComponent(new LevelTag());
        }
    }
}