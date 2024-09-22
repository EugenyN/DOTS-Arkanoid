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
    PowerUpsCount
}

public struct PowerUpData : IComponentData
{
    public PowerUpType Type;
}