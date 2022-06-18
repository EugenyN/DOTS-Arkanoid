using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;

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
        ecb.DestroyEntitiesForEntityQuery(_despawnQuery);
        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}