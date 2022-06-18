using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class TextureAnimationSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem _beginSimulationEcbSystem;

    protected override void OnCreate()
    {
        base.OnCreate();
        _beginSimulationEcbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = _beginSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

        var deltaTime = Time.DeltaTime;

        Entities
            .WithNone<PlayTextureAnimation>()
            .ForEach((Entity entity, ref TextureAnimationData animation, ref MaterialTextureSTData textureSt,
                in TextureSheetConfig config) =>
            {
                textureSt.Value = GetTextureOffset(animation.FrameIndex, config);
            }).ScheduleParallel();

        Entities
            .ForEach((Entity entity, int entityInQueryIndex, ref TextureAnimationData animation,
                ref MaterialTextureSTData textureSt, ref PlayTextureAnimation playAnimation,
                in TextureSheetConfig config) =>
            {
                animation.Time += deltaTime;
                if (animation.Time >= playAnimation.FrameTime)
                {
                    if (!playAnimation.Initialized)
                    {
                        animation.FrameIndex = playAnimation.StartFrame;
                        animation.IndexDecrement = false;
                        playAnimation.Initialized = true;
                    }

                    animation.Time = 0.0f;

                    textureSt.Value = GetTextureOffset(animation.FrameIndex, config);

                    if (playAnimation.Type == TextureAnimationType.PingPong)
                    {
                        animation.FrameIndex += animation.IndexDecrement ? -1 : 1;
                        if (animation.FrameIndex == playAnimation.StartFrame + playAnimation.FramesCount - 1 ||
                            animation.FrameIndex == playAnimation.StartFrame)
                        {
                            animation.IndexDecrement = !animation.IndexDecrement;
                        }
                    }
                    else
                    {
                        animation.FrameIndex++;
                        if (animation.FrameIndex > playAnimation.StartFrame + playAnimation.FramesCount - 1)
                        {
                            animation.FrameIndex = playAnimation.StartFrame;
                            if (playAnimation.Type == TextureAnimationType.Once)
                                ecb.RemoveComponent<PlayTextureAnimation>(entityInQueryIndex, entity);
                        }
                    }
                }
            }).ScheduleParallel();

        _beginSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }

    private static float4 GetTextureOffset(int frameIndex, in TextureSheetConfig config)
    {
        var offset = new float2((config.FrameColumns - 1) - (frameIndex % config.FrameColumns),
                                (frameIndex / config.FrameColumns) % config.FrameRows);
        
        var spriteSize = 1f / new float2(config.FrameColumns, config.FrameRows);
        
        return new float4(spriteSize, offset * spriteSize) * -1;
    }
}