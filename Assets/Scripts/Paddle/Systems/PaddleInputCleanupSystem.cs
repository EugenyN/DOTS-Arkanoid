using Unity.Entities;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial class PaddleInputCleanupSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref PaddleInputData inputData) =>
        {
            inputData.Movement = 0;
            inputData.Action = InputActionType.None;
        }).Schedule();
    }
}