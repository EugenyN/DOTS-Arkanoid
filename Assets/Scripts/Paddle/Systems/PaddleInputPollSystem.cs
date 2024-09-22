using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;

[UpdateInGroup(typeof(GameInputSystemGroup))]
public partial struct PaddleInputPollSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerData>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var inputSettings = SystemAPI.ManagedAPI.GetSingleton<InputSettings>();

        foreach (var (inputData, playerIndex) in SystemAPI.Query<RefRW<PaddleInputData>, RefRO<PlayerIndex>>())
        {
            var inputNames = inputSettings.InputNames[playerIndex.ValueRO.Value];
                    
#if UNITY_STANDALONE
            if (playerIndex.ValueRO.Value == 0)
            {
                inputData.ValueRW.Movement += Input.GetAxis(inputNames.MouseMove);
                if (Input.GetButtonDown(inputNames.MouseAction) && !IsPointerOverGameObject())
                    inputData.ValueRW.Action = InputActionType.Fire;
            }
#else
                if (playerIndex.ValueRO.Value == 0)
                {
                    if (Input.GetMouseButton(0) && !IsPointerOverGameObject())
                        inputData.ValueRW.Movement = Input.mousePosition.x > Screen.width / 2.0f ? 1 : -1;
                }
#endif
            inputData.ValueRW.Movement += Input.GetAxis(inputNames.Move);
                
            if (Input.GetButtonDown(inputNames.Pause))
                inputData.ValueRW.Action = InputActionType.Pause;
                
            if (Input.GetButtonDown(inputNames.Action))
                inputData.ValueRW.Action = InputActionType.Fire;
                
            if (Input.GetKeyDown(KeyCode.F1))
                inputData.ValueRW.Action = InputActionType.SpawnBallCheat;
        }
    }
    
    public static void DoInputAction(EntityManager entityManager, int playerIndex, InputActionType inputAction)
    {
        var paddlesQuery = new EntityQueryBuilder(Allocator.Temp).WithAllRW<PaddleInputData>().Build(entityManager);
        using var paddles = paddlesQuery.ToEntityArray(Allocator.Temp);
        foreach (var paddle in paddles)
        {
            var index = entityManager.GetComponentData<PlayerIndex>(paddle);
            if (index.Value == playerIndex)
            {
                var inputData = entityManager.GetComponentData<PaddleInputData>(paddle);
                inputData.Action = inputAction;
                entityManager.SetComponentData(paddle, inputData);
            }
        }
    }
    
    private static bool IsPointerOverGameObject()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return true;
    
        for (int i = 0; i < Input.touchCount; i++)
        {
            var touch = Input.GetTouch(i);
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                return true;
        }
    
        return false;
    }
}