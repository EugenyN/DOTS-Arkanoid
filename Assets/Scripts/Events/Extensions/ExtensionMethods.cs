using Unity.Entities;

public static class ExtensionMethods
{
    public static void AddSingleFrameComponent<T>(this EntityCommandBuffer ecb, T component)
        where T : unmanaged, IComponentData
    {
        var entity = ecb.CreateEntity();
#if DEBUG
        ecb.SetName(entity, $"Event<{ComponentType.ReadOnly<T>()}>");
#endif

        ecb.AddComponent(entity, component);
        ecb.AddComponent<SingleFrameEntityTag>(entity);
    }
    
    public static void AddSingleFrameComponent<T>(this EntityManager em, T component)
        where T : unmanaged, IComponentData
    {
        var entity = em.CreateEntity();
#if DEBUG
        em.SetName(entity, $"Event<{ComponentType.ReadOnly<T>()}>");
#endif

        em.AddComponentData(entity, component);
        em.AddComponent<SingleFrameEntityTag>(entity);
    }
    
    public static void AddSingleFrameComponent<T>(this EntityCommandBuffer ecb) where T : unmanaged, IComponentData
    {
        var entity = ecb.CreateEntity();
#if DEBUG
        ecb.SetName(entity, $"Event<{ComponentType.ReadOnly<T>()}>");
#endif

        ecb.AddComponent<T>(entity);
        ecb.AddComponent<SingleFrameEntityTag>(entity);
    }
    
    public static void AddSingleFrameComponent<T>(this EntityCommandBuffer ecb, Entity entity, T component)
        where T : unmanaged, IComponentData
    {
        ecb.AddComponent(entity, component);
        ecb.AppendToBuffer(entity, new SingleFrameComponent { TargetComponent = ComponentType.ReadOnly<T>() });
    }

    public static void AddSingleFrameComponent<T>(this EntityManager em, Entity entity, T component)
        where T : unmanaged, IComponentData
    {
        em.AddComponentData(entity, component);
        var buffer = em.GetBuffer<SingleFrameComponent>(entity);
        buffer.Add(new SingleFrameComponent { TargetComponent = ComponentType.ReadOnly<T>() });
    }
    
    public static void AddSingleFrameComponent<T>(this EntityCommandBuffer ecb, Entity entity)
        where T : unmanaged, IComponentData
    {
        ecb.AddComponent<T>(entity);
        ecb.AppendToBuffer(entity, new SingleFrameComponent { TargetComponent = ComponentType.ReadOnly<T>() });
    }
}