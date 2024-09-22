using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup))]
public partial struct PaddleMovementSystem : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<LevelsSettings>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var levelsSettings = SystemAPI.GetSingleton<LevelsSettings>();

		new PaddleMovementJob
		{
			GameAreaWidth = levelsSettings.GameAreaWidth,
			DeltaTime = SystemAPI.Time.DeltaTime
		}.Schedule();
	}
	
	[BurstCompile]
	public partial struct PaddleMovementJob : IJobEntity
	{
		public int GameAreaWidth;
		public float DeltaTime;
        
		private void Execute(ref LocalTransform transform, in PaddleInputData inputData, in PaddleData paddleData)
		{
			if (inputData.Movement != 0)
			{
				float leftBound = 1 + paddleData.Size.x / 2.0f;
				float rightBound = GameAreaWidth - 1 - paddleData.Size.x / 2.0f;
				float movement = paddleData.Speed * inputData.Movement * DeltaTime;
				transform.Position.x = math.clamp(transform.Position.x + movement, leftBound, rightBound);
			}
		}
	}
}