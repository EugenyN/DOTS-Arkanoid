using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;

[UpdateInGroup(typeof(GameInputSystemGroup))]
public partial class PaddleInputPollSystem : SystemBase
{
    private EntityQuery _paddlesQuery;
    
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireForUpdate(GetEntityQuery(typeof(PlayerData)));
    }

    protected override void OnUpdate()
    {
        var inputSettings = SystemAPI.ManagedAPI.GetSingleton<InputSettings>();
        
        Entities
            .WithoutBurst()
            .WithStoreEntityQueryInField(ref _paddlesQuery)
            .ForEach((ref PaddleInputData inputData, in PlayerIndex playerIndex) =>
            {
                var inputNames = inputSettings.InputNames[playerIndex.Value];
                    
#if UNITY_STANDALONE
                if (playerIndex.Value == 0)
                {
                    inputData.Movement += Input.GetAxis(inputNames.MouseMove);
                    if (Input.GetButtonDown(inputNames.MouseAction) && !IsPointerOverGameObject())
                        inputData.Action = InputActionType.Fire;
                }
#else
                if (playerIndex.Value == 0)
                {
                    if (Input.GetMouseButton(0) && !IsPointerOverGameObject())
                        inputData.Movement = Input.mousePosition.x > Screen.width / 2.0f ? 1 : -1;
                }
#endif
                inputData.Movement += Input.GetAxis(inputNames.Move);
                
                if (Input.GetButtonDown(inputNames.Pause))
                    inputData.Action = InputActionType.Pause;
                
                if (Input.GetButtonDown(inputNames.Action))
                    inputData.Action = InputActionType.Fire;
                
                if (Input.GetKeyDown(KeyCode.F1))
                    inputData.Action = InputActionType.SpawnBallCheat;
            }).Run();
    }

    public void DoInputAction(int playerIndex, InputActionType inputAction)
    {
        using var paddles = _paddlesQuery.ToEntityArray(Allocator.Temp);
        foreach (var paddle in paddles)
        {
            var index = SystemAPI.GetComponent<PlayerIndex>(paddle);
            if (index.Value == playerIndex)
            {
                var inputData = SystemAPI.GetComponent<PaddleInputData>(paddle);
                inputData.Action = inputAction;
                SystemAPI.SetComponent(paddle, inputData);
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