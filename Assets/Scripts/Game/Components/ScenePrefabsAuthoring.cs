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
            AddComponent(new ScenePrefabs
            {
                BallEntityPrefab = GetEntity(authoring.BallEntityPrefab),
                BlockEntityPrefab = GetEntity(authoring.BlockEntityPrefab),
                PaddleEntityPrefab = GetEntity(authoring.PaddleEntityPrefab),
                PowerUpEntityPrefab = GetEntity(authoring.PowerUpEntityPrefab),
                LevelEntityPrefab = GetEntity(authoring.LevelEntityPrefab),
                LaserEntityPrefab = GetEntity(authoring.LaserEntityPrefab)
            });
        }
    }
}