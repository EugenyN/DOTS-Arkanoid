using System;
using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(GameStateSystemGroup))]
public partial class GameProcessSystem : SystemBase
{
	protected override void OnCreate()
	{
		base.OnCreate();
		RequireForUpdate<GameProcessState>();
	}
	
	protected override void OnStartRunning()
    {
	    var ecb = new EntityCommandBuffer(Allocator.TempJob);

	    var prefabs = SystemAPI.GetSingleton<ScenePrefabs>();
		
	    ecb.Instantiate(prefabs.LevelEntityPrefab);
	    
	    var playerDataQuery = GetEntityQuery(typeof(PlayerData));
	    if (playerDataQuery.IsEmpty)
	    {
		    var gameData = SystemAPI.GetSingleton<GameData>();
		    for (int i = 0; i < gameData.PlayersCount; i++)
			    CreatePlayer(ecb, i);
	    }

	    ecb.Playback(EntityManager);
	    ecb.Dispose();
	    
	    ecb = new EntityCommandBuffer(Allocator.TempJob);

	    using var playerDataEntities = playerDataQuery.ToEntityArray(Allocator.Temp);
	    foreach (var player in playerDataEntities)
	    {
		    var playerData = SystemAPI.GetComponent<PlayerData>(player);
		    if (playerData.Lives != 0)
			    ecb.AddSingleFrameComponent(new PaddleSpawnRequest { OwnerPlayer = player });
	    }
	    
	    ecb.AddSingleFrameComponent(new BlocksSpawnRequest { BlockPrefab = prefabs.BlockEntityPrefab });
	    
	    var levelsSettings = SystemAPI.ManagedAPI.GetSingleton<LevelsSettings>();
	    SetBallSpeedForLevel(levelsSettings);
	    
	    AudioSystem.PlayAudio(ecb, AudioClipKeys.RoundStart);
	    
	    ecb.Playback(EntityManager);
	    ecb.Dispose();
    }
	
	protected override void OnUpdate()
	{
	}
	
	private Entity CreatePlayer(EntityCommandBuffer ecb, int playerIndex)
	{
		var gameSettings = SystemAPI.GetSingleton<GameSettings>();
		var entity = ecb.CreateEntity();
		ecb.SetName(entity, "Player" + playerIndex);

		ecb.AddComponent(entity, new PlayerData { Lives = gameSettings.PlayerStartLives, Score = 0 });
		ecb.AddComponent(entity, new PlayerIndex { Value = playerIndex});
		ecb.AddBuffer<SingleFrameComponent>(entity);
		
		return entity;
	}
	
	private void SetBallSpeedForLevel(LevelsSettings levelsSettings)
	{
		var gameData = SystemAPI.GetSingleton<GameData>();
		var gameSettings = SystemAPI.GetSingleton<GameSettings>();
        
		int levelsCount = levelsSettings.LevelsData.Length;
		int levelDifficulty = gameData.Level / (levelsCount + 1);
		gameData.BallSpeed = gameSettings.BallSpeed + 1.2f * levelDifficulty;
        
		SystemAPI.SetSingleton(gameData);
	}
}