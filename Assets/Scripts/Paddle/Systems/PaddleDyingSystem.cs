using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup))]
public partial struct PaddleDyingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        
        new PaddleDyingJob
        {
            Ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged),
            PlayerDataLookup = SystemAPI.GetComponentLookup<PlayerData>(),
            DeltaTime = SystemAPI.Time.DeltaTime
        }.Schedule();
    }
    
    [BurstCompile]
    public partial struct PaddleDyingJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;
        public ComponentLookup<PlayerData> PlayerDataLookup;
        public float DeltaTime;
        
        private void Execute(Entity paddle, ref PaddleDyingStateData dyingPaddle, ref MaterialColorData materialColor,
            in OwnerPlayerId ownerPlayerId)
        {
                if (dyingPaddle.StateTimer > 0)
                {
                    dyingPaddle.StateTimer -= DeltaTime;
                    return;
                }

                var playerData = PlayerDataLookup[ownerPlayerId.Value];
                
                switch (dyingPaddle.State)
                {
                    case PaddleDyingState.Dying:
                        
                        Ecb.RemoveComponent<PhysicsCollider>(paddle);
                        Ecb.RemoveComponent<PaddleInputData>(paddle);
                        Ecb.RemoveComponent<PlayTextureAnimation>(paddle);
                        
                        AudioSystem.PlayAudio(Ecb, AudioClipKeys.BallLoss);

                        materialColor.Value *= 0.5f;
                        
                        dyingPaddle.State = PaddleDyingState.DyingComplete;
                        dyingPaddle.StateTimer = 1.0f; //TODO: const
                        break;
                    case PaddleDyingState.DyingComplete:
                        
                        Ecb.RemoveComponent<LocalToWorld>(paddle); // hide entity
                        
                        dyingPaddle.State = PaddleDyingState.RespawnOrGameOver;
                        
                        if (playerData.Lives > 1)
                            dyingPaddle.StateTimer = 2.0f; //TODO: const
                        break;
                    case PaddleDyingState.RespawnOrGameOver:
                        playerData.Lives--;
                        PlayerDataLookup[ownerPlayerId.Value] = playerData;
                        
                        if (playerData.Lives != 0)
                            Ecb.AddSingleFrameComponent(new PaddleSpawnRequest { OwnerPlayer = ownerPlayerId.Value });

                        Ecb.DestroyEntity(paddle);
                        
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
        }
    }
}