using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct SingleFrameEntitySystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        
        if (!SystemAPI.HasSingleton<EventsHolderTag>())
        {
            var entity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponent<EventsHolderTag>(entity);
            state.EntityManager.AddBuffer<SingleFrameComponent>(entity);
        }
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        
        var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
        
        foreach (var (_, entity) in SystemAPI.Query<RefRO<SingleFrameEntityTag>>().WithEntityAccess())
            ecb.DestroyEntity(entity);
       
        foreach (var (components, entity) in SystemAPI.Query<DynamicBuffer<SingleFrameComponent>>().WithEntityAccess())
        {
            if (components.Length > 0)
            {
                for (int i = components.Length - 1; i >= 0; i--)
                {
                    ecb.RemoveComponent(entity, components[i].TargetComponent);
                    components.RemoveAt(i);
                }
            }
        }
    }
}