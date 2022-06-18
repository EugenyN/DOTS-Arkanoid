using Unity.Entities;

public enum PowerUpType
{
    Break,
    Catch,
    Disruption,
    Enlarge,
    Laser,
    MegaBall,
    Player,
    Slow,
    /*Twin*/
}

public struct PowerUpData : IComponentData
{
    public PowerUpType Type;
}