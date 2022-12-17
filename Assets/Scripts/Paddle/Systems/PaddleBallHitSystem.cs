using Unity.Collections;
using Unity.Entities;

public partial class PaddleBallHitSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
    
    protected override void OnCreate()
    {
        base.OnCreate();
        
        _endSimulationEcbSystem = World.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
        
        RequireForUpdate(GetEntityQuery(typeof(HitByBallEvent), typeof(PaddleData)));
    }

    protected override void OnUpdate()
    {
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();
        
        Entities
            .ForEach((Entity paddle, in HitByBallEvent hitByBall, in OwnerPlayerId ownerPlayerId) =>
            {
                var playerData = SystemAPI.GetComponent<PlayerData>(ownerPlayerId.Value);
                playerData.Score += 10;
                SystemAPI.SetComponent(ownerPlayerId.Value, playerData);

                AudioSystem.PlayAudio(ecb, SystemAPI.HasComponent<StickPaddleTag>(paddle) ? 
                    AudioClipKeys.PaddleCatch : AudioClipKeys.PaddleHit);
            }).Schedule();
        
        _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }
}