using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
[UpdateAfter(typeof(BeginSimulationEntityCommandBufferSystem))]
[UpdateBefore(typeof(VariableRateSimulationSystemGroup))]
public class GameStateSystemGroup : ComponentSystemGroup {}

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
[UpdateAfter(typeof(GameStateSystemGroup))]
[UpdateBefore(typeof(VariableRateSimulationSystemGroup))]
public class GameInputSystemGroup : ComponentSystemGroup {}

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
[UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(VariableRateSimulationSystemGroup))]
public class BallBlockPaddleSystemGroup : ComponentSystemGroup {}

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
[UpdateAfter(typeof(BallBlockPaddleSystemGroup))]
[UpdateBefore(typeof(VariableRateSimulationSystemGroup))]
public class PowerUpsSystemGroup : ComponentSystemGroup {}