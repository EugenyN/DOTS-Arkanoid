using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(GameInputSystemGroup))]
public partial struct PaddleAIInputSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameSettings>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var gameSettings = SystemAPI.GetSingleton<GameSettings>();
        
        foreach (var (inputData, playerIndex) in SystemAPI.Query<RefRW<PaddleInputData>, RefRO<PlayerIndex>>())
        {
            if (playerIndex.ValueRO.Value > 1)
            {
                var side = (int) SystemAPI.Time.ElapsedTime % (2 + playerIndex.ValueRO.Value);
                inputData.ValueRW.Movement += (side == 0 ? 1 : -1) * (0.5f + gameSettings.InputSensitivity);
            }
        }
    }
}