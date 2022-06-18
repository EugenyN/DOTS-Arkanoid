using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

[DisableAutoCreation] // Comment to activate cheat
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(BuildPhysicsWorld))]
public partial class BallProtectionCheatSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities
            .WithAny<BallData>()
            .ForEach((Entity entity, ref PhysicsVelocity velocity, in Translation trans) =>
        {
            if (trans.Value.y <= 1)
                velocity.Linear.y *= -1;
        }).Schedule();
    }
}