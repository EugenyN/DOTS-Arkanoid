using Unity.Entities;

public partial class BreakPowerUpSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
    
    protected override void OnCreate()
    {
        base.OnCreate();
        
        _endSimulationEcbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        
        RequireForUpdate<PowerUpReceivedEvent>();
    }
    
    protected override void OnUpdate()
    {
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();
        
        Entities
            .ForEach((in PowerUpReceivedEvent request) =>
            {
                if (request.Type == PowerUpType.Break)
                {
                    ecb.AddSingleFrameComponent(new ChangeStateCommand {
                        TargetState = ComponentType.ReadWrite<GameWinState>()
                    });
                }
            }).Schedule();
        
        _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }
}