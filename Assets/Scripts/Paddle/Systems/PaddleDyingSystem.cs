using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

public partial class PaddleDyingSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
    
    protected override void OnCreate()
    {
        _endSimulationEcbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }
    
    protected override void OnUpdate()
    {
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();

        var deltaTime = Time.DeltaTime;
        
        Entities
            .ForEach((Entity entity, ref PaddleDyingStateData dyingPaddle, ref MaterialColorData materialColor,
                in OwnerPlayerId ownerPlayerId) =>
            {
                if (dyingPaddle.StateTimer > 0)
                {
                    dyingPaddle.StateTimer -= deltaTime;
                    return;
                }

                var playerData = GetComponent<PlayerData>(ownerPlayerId.Value);
                
                switch (dyingPaddle.State)
                {
                    case PaddleDyingState.Dying:
                        
                        ecb.RemoveComponent<PhysicsCollider>(entity);
                        ecb.RemoveComponent<PaddleInputData>(entity);
                        ecb.RemoveComponent<PlayTextureAnimation>(entity);
                        
                        AudioSystem.PlayAudio(ecb, AudioClipKeys.BallLoss);

                        materialColor.Value *= 0.5f;
                        
                        dyingPaddle.State = PaddleDyingState.DyingComplete;
                        dyingPaddle.StateTimer = 1.0f; //TODO: const
                        break;
                    case PaddleDyingState.DyingComplete:
                        
                        ecb.RemoveComponent<LocalToWorld>(entity); // hide entity
                        
                        dyingPaddle.State = PaddleDyingState.RespawnOrGameOver;
                        
                        if (playerData.Lives > 1)
                            dyingPaddle.StateTimer = 2.0f; //TODO: const
                        break;
                    case PaddleDyingState.RespawnOrGameOver:
                        playerData.Lives--;
                        SetComponent(ownerPlayerId.Value, playerData);
                        
                        if (playerData.Lives != 0)
                            ecb.AddSingleFrameComponent(new PaddleSpawnRequest { OwnerPlayer = ownerPlayerId.Value });

                        ecb.DestroyEntity(entity);
                        
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }).Schedule();
        
        _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }
}