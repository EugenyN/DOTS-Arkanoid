using Unity.Entities;

public struct BallLink : IBufferElementData
{
    public Entity Ball;
    
    public static implicit operator BallLink(Entity e)
    {
        return new BallLink {Ball = e};
    }
}