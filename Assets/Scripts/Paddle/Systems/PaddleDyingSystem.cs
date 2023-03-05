using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup))]
public partial class PaddleDyingSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
    
    protected override void OnCreate()
    {
        _endSimulationEcbSystem = World.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
    }
    
    protected override void OnUpdate()
    {
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();

        var deltaTime = World.Time.DeltaTime;
        
        Entities
            .ForEach((Entity paddle, ref PaddleDyingStateData dyingPaddle, ref MaterialColorData materialColor,
                in OwnerPlayerId ownerPlayerId) =>
            {
                if (dyingPaddle.StateTimer > 0)
                {
                    dyingPaddle.StateTimer -= deltaTime;
                    return;
                }

                var playerData = SystemAPI.GetComponent<PlayerData>(ownerPlayerId.Value);
                
                switch (dyingPaddle.State)
                {
                    case PaddleDyingState.Dying:
                        
                        ecb.RemoveComponent<PhysicsCollider>(paddle);
                        ecb.RemoveComponent<PaddleInputData>(paddle);
                        ecb.RemoveComponent<PlayTextureAnimation>(paddle);
                        
                        AudioSystem.PlayAudio(ecb, AudioClipKeys.BallLoss);

                        materialColor.Value *= 0.5f;
                        
                        dyingPaddle.State = PaddleDyingState.DyingComplete;
                        dyingPaddle.StateTimer = 1.0f; //TODO: const
                        break;
                    case PaddleDyingState.DyingComplete:
                        
                        ecb.RemoveComponent<LocalToWorld>(paddle); // hide entity
                        
                        dyingPaddle.State = PaddleDyingState.RespawnOrGameOver;
                        
                        if (playerData.Lives > 1)
                            dyingPaddle.StateTimer = 2.0f; //TODO: const
                        break;
                    case PaddleDyingState.RespawnOrGameOver:
                        playerData.Lives--;
                        SystemAPI.SetComponent(ownerPlayerId.Value, playerData);
                        
                        if (playerData.Lives != 0)
                            ecb.AddSingleFrameComponent(new PaddleSpawnRequest { OwnerPlayer = ownerPlayerId.Value });

                        ecb.DestroyEntity(paddle);
                        
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }).Schedule();
        
        _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }
}