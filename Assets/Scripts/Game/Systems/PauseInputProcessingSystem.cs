using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup))]
public partial struct PauseInputProcessingSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var inputData in SystemAPI.Query<RefRO<PaddleInputData>>())
        {
            if (inputData.ValueRO.Action == InputActionType.Pause)
                GameSystem.SetPause(state.EntityManager, !GameSystem.IsGamePaused());
        }
    }
}