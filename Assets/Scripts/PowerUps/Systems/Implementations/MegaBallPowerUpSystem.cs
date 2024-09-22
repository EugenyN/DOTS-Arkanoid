using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(PowerUpsSystemGroup))]
public partial struct MegaBallPowerUpSystem : ISystem
{
    private static readonly float4 MegaBallColor = new float4(0.0f, 1.0f, 0.0f, 1.0f);
    private static readonly float4 NormalBallColor = new float4(1.0f, 1.0f, 1.0f, 1.0f);
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        
        new MegaBallPowerUpJob
        {
            Ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged),
            MegaBallTagLookup = SystemAPI.GetComponentLookup<MegaBallTag>(true),
            MaterialColorDataLookup = SystemAPI.GetComponentLookup<MaterialColorData>()
        }.Schedule();
    }
    
    [BurstCompile]
    public partial struct MegaBallPowerUpJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;
        [ReadOnly] public ComponentLookup<MegaBallTag> MegaBallTagLookup;
        public ComponentLookup<MaterialColorData> MaterialColorDataLookup;
        
        private void Execute(in PowerUpReceivedEvent request, in DynamicBuffer<BallLink> ballsBuffer)
        {
            if (request.Type == PowerUpType.Disruption)
            {
                foreach (var ball in ballsBuffer.Reinterpret<Entity>())
                {
                    if (MegaBallTagLookup.HasComponent(ball))
                    {
                        var materialColor = MaterialColorDataLookup[ball];
                        materialColor.Value = NormalBallColor;
                        MaterialColorDataLookup[ball] = materialColor;

                        Ecb.RemoveComponent<MegaBallTag>(ball);
                    }
                }
            }
                    
            if (request.Type == PowerUpType.MegaBall)
            {
                foreach (var ball in ballsBuffer.Reinterpret<Entity>())
                {
                    var materialColor = MaterialColorDataLookup[ball];
                    materialColor.Value = MegaBallColor;
                    MaterialColorDataLookup[ball] = materialColor;
                        
                    Ecb.AddComponent<MegaBallTag>(ball);
                    break;
                }
            }
        }
    }
}