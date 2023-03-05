using Unity.Collections;
using Unity.Entities;
using Unity.Physics;

[UpdateInGroup(typeof(GameStateSystemGroup))]
public partial class GameWinSystem : SystemBase
{
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireForUpdate<GameWinState>();
    }
    
    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        
        var gameState = SystemAPI.GetSingleton<GameStateData>();
        gameState.StateTimer = 3.0f;
        SystemAPI.SetSingleton(gameState);
        
        var ecb = new EntityCommandBuffer(Allocator.TempJob);

        Entities.WithAny<PaddleData>().ForEach((Entity entity) =>
        {
            ecb.RemoveComponent<PhysicsCollider>(entity);
            ecb.RemoveComponent<PaddleInputData>(entity);
        }).Run();

        Entities.WithAny<PhysicsVelocity>().ForEach((Entity entity) =>
        {
            ecb.RemoveComponent<PhysicsVelocity>(entity);
        }).Run();

        Entities.WithAny<PlayTextureAnimation>().ForEach((Entity entity) =>
        {
            ecb.RemoveComponent<PlayTextureAnimation>(entity);
        }).Run();
            
        ecb.Playback(EntityManager);
        ecb.Dispose();
        
        AudioSystem.PlayAudio(EntityManager, AudioClipKeys.RoundCleared);
    }

    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.TempJob);

        var deltaTime = World.Time.DeltaTime;
        
        Entities.ForEach((Entity entity, ref GameData gameData, ref GameStateData gameStateData) =>
        {
            if (gameStateData.StateTimer > 0)
                gameStateData.StateTimer -= deltaTime;
            else
            {
                gameData.Level++;
            
                ecb.AddSingleFrameComponent(new LevelDespawnRequest());
                ecb.AddSingleFrameComponent(new ChangeStateCommand
                {
                    TargetState = ComponentType.ReadWrite<GameProcessState>()
                });
            }
        }).Run();
        
        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}