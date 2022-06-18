using Unity.Mathematics;

public static class MathUtils
{
    public static float3 ClampMagnitude(float3 vector, float maxLength)
    {
        float sqrMagnitude = math.lengthsq(vector);
 
        if (sqrMagnitude <= (double)maxLength * maxLength)
            return vector;
 
        float num1 = math.sqrt(sqrMagnitude);
        vector /= num1;
        vector *= maxLength;
        return vector;
    }
}