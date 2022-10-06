using Unity.Entities;
using Unity.Mathematics;

public struct GameSettings : IComponentData
{
    public int StartLevel;
    public int HighScore;
    public float BallSpeed;
    public float BallSpeedIncreaseFactor;
    public float BallMovingDelay;
    public int PlayerStartLives;
    public int PlayerMaxLives;
    public int PowerUpScore;
    public int BreakPowerUpScore;
    public float PowerUpProbability;
    public int DisruptionPowerUpBallsCount;
    public float3 PaddleSize;
    public float3 BigPaddleSize;
    public float PaddleSpeed;
}