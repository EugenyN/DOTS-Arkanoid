using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup))]
public partial struct PauseInputProcessingSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        
        foreach (var inputData in SystemAPI.Query<RefRO<PaddleInputData>>())
        {
            if (inputData.ValueRO.Action == InputActionType.Pause)
            {
                GameSystem.SetPause(ecb, !GameSystem.IsGamePaused());
                break;
            }
        }
        
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}