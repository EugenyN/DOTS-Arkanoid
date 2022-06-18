using Unity.Entities;

public struct BallStuckToPaddle : IComponentData
{
    public float StuckTime;
    public float Offset;
}