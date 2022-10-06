using Unity.Entities;

public struct ScenePrefabs : IComponentData
{
    public Entity BallEntityPrefab;
    public Entity BlockEntityPrefab;
    public Entity PaddleEntityPrefab;
    public Entity PowerUpEntityPrefab;
    public Entity LevelEntityPrefab;
    public Entity LaserEntityPrefab;
}