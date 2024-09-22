using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup))]
public partial struct StickPaddleSystem : ISystem
{
    private const float StuckTimeLimit = 3.0f;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        
        new StickPaddleJob
        {
            Ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged),
            BallDataLookup = SystemAPI.GetComponentLookup<BallData>(),
            BallStuckToPaddleLookup = SystemAPI.GetComponentLookup<BallStuckToPaddle>(),
            LocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(),
            PaddleDataLookup = SystemAPI.GetComponentLookup<PaddleData>(),
        }.Schedule();
    }
    
    [BurstCompile]
    [WithAll(typeof(StickPaddleTag))]
    public partial struct StickPaddleJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;
        [ReadOnly] public ComponentLookup<BallData> BallDataLookup;
        [ReadOnly] public ComponentLookup<BallStuckToPaddle> BallStuckToPaddleLookup;
        [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
        [ReadOnly] public ComponentLookup<PaddleData> PaddleDataLookup;
        
        private void Execute(Entity paddle, in LocalTransform paddleTransform,
            in HitByBallEvent hitByBallEvent, in DynamicBuffer<BallLink> ballsBuffer)
        {
            var ballData = BallDataLookup[hitByBallEvent.Ball];
            if (ballData.OwnerPaddle == paddle)
            {
                bool hasAny = false;
                for (int i = 0; i < ballsBuffer.Length; i++)
                    hasAny |= BallStuckToPaddleLookup.HasComponent(ballsBuffer[i].Ball);
                if (!hasAny)
                {
                    var ballTransform = LocalTransformLookup[hitByBallEvent.Ball];
                    var paddleData = PaddleDataLookup[paddle];
                    float stickOffset = ballTransform.Position.x - paddleTransform.Position.x;
                    float stickSide = paddleData.Size.x * 0.7f;
                    Ecb.AddComponent(hitByBallEvent.Ball, new BallStuckToPaddle {
                        StuckTime = StuckTimeLimit,
                        Offset = math.clamp(stickOffset, -stickSide / 2, stickSide / 2)
                    });
                }
            }
        }
    }
}