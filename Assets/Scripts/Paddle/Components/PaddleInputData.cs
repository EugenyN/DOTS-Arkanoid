using Unity.Entities;

public enum InputActionType
{
	None,
	Fire,
	Pause,
	Exit,
	SpawnBallCheat
}

public struct PaddleInputData : IComponentData
{
	public float Movement;
	public InputActionType Action;
}
