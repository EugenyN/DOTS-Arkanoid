using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(GameStateSystemGroup))]
public partial class GameOverSystem : SystemBase
{
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireForUpdate<GameOverState>();
    }

    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        
        int highScore = 0;
        Entities.ForEach((in PlayerData playerData) => { highScore = math.max(highScore, playerData.Score); }).Run();
        
        var gameData = SystemAPI.GetSingleton<GameData>();
        if (highScore > gameData.HighScore)
        {
            gameData.HighScore = highScore;
            EntityManager.AddSingleFrameComponent(new HiScoreUpdatedEvent { Score = highScore });
        }
        SystemAPI.SetSingleton(gameData);
        
        var gameState = SystemAPI.GetSingleton<GameStateData>();
        gameState.StateTimer = 5.0f;
        SystemAPI.SetSingleton(gameState);

        AudioSystem.PlayAudio(EntityManager, AudioClipKeys.GameOver);
    }

    protected override void OnUpdate()
    {
        var gameState = SystemAPI.GetSingleton<GameStateData>();

        if (gameState.StateTimer > 0)
        {
            gameState.StateTimer -= World.Time.DeltaTime;
            SystemAPI.SetSingleton(gameState);
        }
        else
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            ecb.AddSingleFrameComponent(new LevelDespawnRequest());
            ecb.DestroyEntity(GetEntityQuery(typeof(PlayerData)));
            ecb.AddSingleFrameComponent(new ChangeStateCommand { TargetState = typeof(MainMenuState) });
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}