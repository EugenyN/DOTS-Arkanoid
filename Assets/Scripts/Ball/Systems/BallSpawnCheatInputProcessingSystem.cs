using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup), OrderFirst = true)]
public partial struct BallSpawnCheatInputProcessingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<GameData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var gameData = SystemAPI.GetSingleton<GameData>();

        var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        
        new BallSpawnCheatInputProcessingJob
        {
            Ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged),
            Random = Random.CreateFromIndex(state.GlobalSystemVersion),
            BallSpeed = gameData.BallSpeed
        }.Schedule();
    }
    
    [BurstCompile]
    [WithNone(typeof(LaserPaddleTag))]
    public partial struct BallSpawnCheatInputProcessingJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;
        public Random Random;
        public float BallSpeed;
        
        private void Execute(Entity paddle, ref PaddleInputData inputData, in LocalTransform transform,
            in OwnerPlayerId ownerPlayerId)
        {
            if (inputData.Action == InputActionType.SpawnBallCheat)
            {
                Ecb.AddSingleFrameComponent(new BallSpawnRequest
                {
                    Position = transform.Position + new float3(0, 1, 0),
                    OwnerPaddle = paddle,
                    OwnerPlayer = ownerPlayerId.Value,
                    StuckToPaddle = false,
                    Velocity = BallsHelper.GetRandomDirection(Random) * BallSpeed
                });
            }
        }
    }
}