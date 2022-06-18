using Unity.Mathematics;

public static class BallsHelper
{
    public static float3 GetRandomDirection(Random random)
    {
        return math.normalize(new float3(random.NextBool() ? -1 : 1, random.NextFloat(0.5f, 0.8f), 0f));
    }
    
    public static float3 GetBounceDirection(float3 ballPosition, float3 paddlePosition, float3 paddleSize)
    {
        var hitFactor = (ballPosition.x - paddlePosition.x) / paddleSize.x;
        var hitMult = 4.0f;
        return math.normalize(new float3(hitFactor * hitMult, 1, 0));
    }
}