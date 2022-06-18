using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
public partial class GameStateChangeSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
    
    protected override void OnCreate()
    {
        base.OnCreate();
        
        _endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        
        RequireSingletonForUpdate<GameStateData>();
        RequireSingletonForUpdate<ChangeStateCommand>();
    }

    protected override void OnUpdate()
    {
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();
        
        var command = GetSingleton<ChangeStateCommand>();
        
        Entities
            .WithoutBurst()
            .ForEach((Entity entity, ref GameStateData state) =>
        {
            ecb.RemoveComponent(entity, state.CurrentState);
            ecb.AddComponent(entity, command.TargetState);

            state.CurrentState = command.TargetState;
            
        }).Schedule();
        
        _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }
}