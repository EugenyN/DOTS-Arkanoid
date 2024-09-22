using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

[DisableAutoCreation] // Comment to activate cheat
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
public partial struct BallProtectionCheatSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new BallProtectionCheatJob().Schedule();
    }
    
    [BurstCompile]
    [WithAny(typeof(BallData))]
    public partial struct BallProtectionCheatJob : IJobEntity
    {
        private void Execute(ref PhysicsVelocity velocity, in LocalTransform transform)
        {
            if (transform.Position.y <= 1)
                velocity.Linear.y *= -1;
        }
    }
}