using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class GameSettingsAuthoring : MonoBehaviour
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

    public class Baker : Baker<GameSettingsAuthoring>
    {
        public override void Bake(GameSettingsAuthoring authoring)
        {
            AddComponent(new GameSettings
            {
                StartLevel = authoring.StartLevel,
                HighScore = authoring.HighScore,
                BallSpeed = authoring.BallSpeed,
                BallSpeedIncreaseFactor = authoring.BallSpeedIncreaseFactor,
                BallMovingDelay = authoring.BallMovingDelay,
                PlayerStartLives = authoring.PlayerStartLives,
                PlayerMaxLives = authoring.PlayerMaxLives,
                PowerUpScore = authoring.PowerUpScore,
                BreakPowerUpScore = authoring.BreakPowerUpScore,
                PowerUpProbability = authoring.PowerUpProbability,
                DisruptionPowerUpBallsCount = authoring.DisruptionPowerUpBallsCount,
                PaddleSize = authoring.PaddleSize,
                BigPaddleSize = authoring.BigPaddleSize,
                PaddleSpeed = authoring.PaddleSpeed
            });
        }
    }
}