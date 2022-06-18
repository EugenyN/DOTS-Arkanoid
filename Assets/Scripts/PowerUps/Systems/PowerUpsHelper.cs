public static class PowerUpsHelper
{
    public static bool IsExclusivePowerUp(PowerUpType powerUpType)
    {
        return powerUpType == PowerUpType.Catch || 
               powerUpType == PowerUpType.Enlarge ||
               powerUpType == PowerUpType.Laser;
    }
}