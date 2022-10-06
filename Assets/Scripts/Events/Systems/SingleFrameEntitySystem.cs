using Unity.Entities;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial class SingleFrameEntitySystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

    protected override void OnCreate()
    {
        _endSimulationEcbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        if (!HasSingleton<EventsHolderTag>())
        {
            var entity = EntityManager.CreateEntity(typeof(EventsHolderTag));
            EntityManager.AddBuffer<SingleFrameComponent>(entity);
        }
    }

    protected override void OnUpdate()
    {
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();

        Entities.WithAll<SingleFrameEntityTag>()
            .ForEach((Entity entity) => { ecb.DestroyEntity(entity); }).Schedule();

        Entities.ForEach((Entity entity, ref DynamicBuffer<SingleFrameComponent> components) =>
        {
            if (components.Length > 0)
            {
                for (int i = components.Length - 1; i >= 0; i--)
                {
                    ecb.RemoveComponent(entity, components[i].TargetComponent);
                    components.RemoveAt(i);
                }
            }
        }).Run();

        _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }
}