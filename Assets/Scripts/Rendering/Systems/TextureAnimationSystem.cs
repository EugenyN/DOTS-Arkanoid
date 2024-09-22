using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial struct TextureAnimationSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        
        new TextureSTJob().Schedule();
        
        new TextureAnimationJob
        {
            Ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
            DeltaTime = SystemAPI.Time.DeltaTime
        }.ScheduleParallel();
    }
    
    [BurstCompile]
    [WithNone(typeof(PlayTextureAnimation))]
    public partial struct TextureSTJob : IJobEntity
    {
        private void Execute(ref TextureAnimationData animation, ref MaterialTextureSTData textureSt,
            in TextureSheetConfig config)
        {
            textureSt.Value = GetTextureOffset(animation.FrameIndex, config);
        }
    }
    
    [BurstCompile]
    public partial struct TextureAnimationJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        public float DeltaTime;
        
        private void Execute(Entity entity, ref TextureAnimationData animation,
            ref MaterialTextureSTData textureSt, ref PlayTextureAnimation playAnimation,
            in TextureSheetConfig config, [ChunkIndexInQuery] int chunkIndex)
        {
            animation.Time += DeltaTime;
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
                            Ecb.RemoveComponent<PlayTextureAnimation>(chunkIndex, entity);
                    }
                }
            }
        }
    }

    private static float4 GetTextureOffset(int frameIndex, in TextureSheetConfig config)
    {
        var offset = new float2((config.FrameColumns - 1) - (frameIndex % config.FrameColumns),
                                (int)(frameIndex / config.FrameColumns) % config.FrameRows);
        
        var spriteSize = 1f / new float2(config.FrameColumns, config.FrameRows);
        
        return new float4(spriteSize, offset * spriteSize) * -1;
    }
}