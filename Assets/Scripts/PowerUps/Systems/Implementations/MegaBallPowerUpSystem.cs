using Unity.Entities;
using Unity.Mathematics;

public partial class MegaBallPowerUpSystem : SystemBase
{
    private static readonly float4 MegaBallColor = new float4(0.0f, 1.0f, 0.0f, 1.0f);
    private static readonly float4 NormalBallColor = new float4(1.0f, 1.0f, 1.0f, 1.0f);

    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
    
    protected override void OnCreate()
    {
        base.OnCreate();
        
        _endSimulationEcbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        
        RequireForUpdate<PowerUpReceivedEvent>();
    }
    
    protected override void OnUpdate()
    {
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();
        
        Entities
            .ForEach((Entity paddle, in PowerUpReceivedEvent request, in DynamicBuffer<BallLink> ballsBuffer) =>
            {
                if (request.Type == PowerUpType.Disruption)
                {
                    foreach (var ball in ballsBuffer.Reinterpret<Entity>())
                    {
                        if (SystemAPI.HasComponent<MegaBallTag>(ball))
                        {
                            var materialColor = SystemAPI.GetComponent<MaterialColorData>(ball);
                            materialColor.Value = NormalBallColor;
                            SystemAPI.SetComponent(ball, materialColor);

                            ecb.RemoveComponent<MegaBallTag>(ball);
                        }
                    }
                }
                    
                if (request.Type == PowerUpType.MegaBall)
                {
                    foreach (var ball in ballsBuffer.Reinterpret<Entity>())
                    {
                        var materialColor = SystemAPI.GetComponent<MaterialColorData>(ball);
                        materialColor.Value = MegaBallColor;
                        SystemAPI.SetComponent(ball, materialColor);
                        
                        ecb.AddComponent<MegaBallTag>(ball);
                        break;
                    }
                }
            }).Schedule();
        
        _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }
}