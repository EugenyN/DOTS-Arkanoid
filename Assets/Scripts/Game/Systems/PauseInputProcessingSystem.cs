using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
[UpdateAfter(typeof(PaddleInputPollSystem))]
public partial class PauseInputProcessingSystem : SystemBase
{
    private GameSystem _gameSystem;

    protected override void OnCreate()
    {
        _gameSystem = World.GetExistingSystemManaged<GameSystem>();
    }

    protected override void OnUpdate()
    {
        Entities
            .WithoutBurst()
            .ForEach((Entity paddle, ref PaddleInputData inputData) =>
            {
                if (inputData.Action == InputActionType.Pause)
                    _gameSystem.SetPause(!_gameSystem.IsGamePaused());
            }).Run();
    }
}