using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
[UpdateAfter(typeof(PowerUpsSystemGroup))]
[UpdateBefore(typeof(VariableRateSimulationSystemGroup))]
public partial struct LevelDespawnSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<LevelDespawnRequest>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        
        var despawnQuery = SystemAPI.QueryBuilder().WithAny<RenderBounds, LevelTag>().Build();
        ecb.DestroyEntity(despawnQuery, EntityQueryCaptureMode.AtPlayback);
        
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}