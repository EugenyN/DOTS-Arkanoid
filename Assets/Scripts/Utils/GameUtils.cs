using Unity.Collections;
using Unity.Entities;

public static class GameUtils
{
    public static bool TryGetSingletonEntity<T>(EntityManager entityManager, out Entity singletonEntity)
        where T : unmanaged, IComponentData
    {
        var singletonQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<T>().Build(entityManager);
        if (singletonQuery.HasSingleton<T>())
        {
            singletonEntity = singletonQuery.GetSingletonEntity();
            return true;
        }

        singletonEntity = default;
        return false;
    }
    
    public static bool TryGetSingletonEntityManaged<T>(EntityManager entityManager, out Entity singletonEntity)
        where T : class
    {
        var singletonQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<T>().Build(entityManager);
        if (singletonQuery.HasSingleton<T>())
        {
            singletonEntity = singletonQuery.GetSingletonEntity();
            return true;
        }

        singletonEntity = default;
        return false;
    }

    public static bool TryGetSingleton<T>(EntityManager entityManager, out T singleton)
        where T : unmanaged, IComponentData
    {
        var singletonQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<T>().Build(entityManager);
        if (singletonQuery.HasSingleton<T>())
        {
            singleton = singletonQuery.GetSingleton<T>();
            return true;
        }

        singleton = default;
        return false;
    }
    
    public static bool TryGetSingletonRW<T>(EntityManager entityManager, out RefRW<T> singletonRW) 
        where T : unmanaged, IComponentData
    {
        EntityQuery singletonQuery = new EntityQueryBuilder(Allocator.Temp).WithAllRW<T>().Build(entityManager);
        if (singletonQuery.HasSingleton<T>())
        {
            singletonRW = singletonQuery.GetSingletonRW<T>();
            return true;
        }

        singletonRW = default;
        return false;
    }
    
    public static bool TryGetSingletonManaged<T>(EntityManager entityManager, out T singleton)
        where T : class
    {
        var singletonQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<T>().Build(entityManager);
        if (singletonQuery.HasSingleton<T>())
        {
            singleton = singletonQuery.GetSingleton<T>();
            return true;
        }

        singleton = default;
        return false;
    }
}