using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
[UpdateAfter(typeof(BeginSimulationEntityCommandBufferSystem))]
[UpdateBefore(typeof(VariableRateSimulationSystemGroup))]
public partial class GameStateSystemGroup : ComponentSystemGroup {}

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
[UpdateAfter(typeof(GameStateSystemGroup))]
[UpdateBefore(typeof(VariableRateSimulationSystemGroup))]
public partial class GameInputSystemGroup : ComponentSystemGroup {}

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
[UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(VariableRateSimulationSystemGroup))]
public partial class BallBlockPaddleSystemGroup : ComponentSystemGroup {}

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
[UpdateAfter(typeof(BallBlockPaddleSystemGroup))]
[UpdateBefore(typeof(VariableRateSimulationSystemGroup))]
public partial class PowerUpsSystemGroup : ComponentSystemGroup {}