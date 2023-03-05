using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
[UpdateAfter(typeof(PowerUpsSystemGroup))]
[UpdateBefore(typeof(VariableRateSimulationSystemGroup))]
public partial class LevelDespawnSystem : SystemBase
{
    private EntityQuery _despawnQuery;
    
    protected override void OnCreate()
    {
        base.OnCreate();
        
        _despawnQuery = GetEntityQuery(new EntityQueryDesc
        {
            Any = new [] { ComponentType.ReadOnly<RenderBounds>(), ComponentType.ReadOnly<LevelTag>() }
        });
        
        RequireForUpdate(GetEntityQuery(typeof(LevelDespawnRequest)));
    }

    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        ecb.DestroyEntity(_despawnQuery);
        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}