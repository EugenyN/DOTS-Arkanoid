using Unity.Entities;

public struct GameData : IComponentData
{
    public int Level;
    public int HighScore;
    public float BallSpeed;
    public int PlayersCount;
}