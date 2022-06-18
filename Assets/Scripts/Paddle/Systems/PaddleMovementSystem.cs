using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial class PaddleMovementSystem : SystemBase
{
	protected override void OnUpdate()
	{
		float deltaTime = Time.DeltaTime;
		Entities.ForEach((ref Translation position, in PaddleInputData inputData, in PaddleData paddleData) =>
		{
			if (inputData.Movement != 0)
			{
				float leftBound = 1 + paddleData.Size.x / 2.0f;
				float rightBound = GameConst.GameAreaWidth - 1 - paddleData.Size.x / 2.0f;
				float movement = paddleData.Speed * inputData.Movement * deltaTime;
				position.Value.x = math.clamp(position.Value.x + movement, leftBound, rightBound);
			}
		}).Schedule();
	}
}