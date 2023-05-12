using Unity.Entities;
using UnityEngine;

public class ScenePrefabsAuthoring : MonoBehaviour
{
    public GameObject BallEntityPrefab;
    public GameObject BlockEntityPrefab;
    public GameObject PaddleEntityPrefab;
    public GameObject PowerUpEntityPrefab;
    public GameObject LevelEntityPrefab;
    public GameObject LaserEntityPrefab;

    public class Baker : Baker<ScenePrefabsAuthoring>
    {
        public override void Bake(ScenePrefabsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ScenePrefabs
            {
                BallEntityPrefab = GetEntity(authoring.BallEntityPrefab, TransformUsageFlags.Dynamic),
                BlockEntityPrefab = GetEntity(authoring.BlockEntityPrefab, TransformUsageFlags.Dynamic),
                PaddleEntityPrefab = GetEntity(authoring.PaddleEntityPrefab, TransformUsageFlags.Dynamic),
                PowerUpEntityPrefab = GetEntity(authoring.PowerUpEntityPrefab, TransformUsageFlags.Dynamic),
                LevelEntityPrefab = GetEntity(authoring.LevelEntityPrefab, TransformUsageFlags.Dynamic),
                LaserEntityPrefab = GetEntity(authoring.LaserEntityPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}