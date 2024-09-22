using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct PaddleInputCleanupSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new PaddleInputCleanupJob().Schedule();
    }
    
    [BurstCompile]
    public partial struct PaddleInputCleanupJob : IJobEntity
    {
        private void Execute(ref PaddleInputData inputData)
        {
            inputData.Movement = 0;
            inputData.Action = InputActionType.None;
        }
    }
}