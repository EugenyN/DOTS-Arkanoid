using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[UpdateInGroup(typeof(GameInputSystemGroup))]
public partial class PaddleInputPollSystem : SystemBase
{
    private InputSystem_Actions _inputActions;
        
    protected override void OnCreate()
    {
        RequireForUpdate<PlayerData>();

        _inputActions = new InputSystem_Actions();
    }

    protected override void OnStartRunning()
    {
        _inputActions.Enable();
    }

    protected override void OnStopRunning()
    {
        _inputActions.Disable();
    }

    protected override void OnDestroy()
    {
        _inputActions.Disable();
        _inputActions.Dispose();
    }

    protected override void OnUpdate()
    {
        var gameSettings = SystemAPI.GetSingleton<GameSettings>();

        var inputActions = _inputActions.Gameplay;

        foreach (var (inputData, playerIndex) in SystemAPI.Query<RefRW<PaddleInputData>, RefRO<PlayerIndex>>())
        {
            if (inputActions.Pause.WasPressedThisFrame())
            {
                inputData.ValueRW.Action = InputActionType.Pause;
                break;
            }

            if (GameSystem.IsGamePaused()) 
                continue;
            
            GetInputActions(playerIndex.ValueRO.Value, out var moveInput, out var actionPressed);
            
            inputData.ValueRW.Movement += moveInput.x * (0.5f + gameSettings.InputSensitivity);
            
            if (actionPressed)
                inputData.ValueRW.Action = InputActionType.Fire;
            
            if (inputActions.SpawnCheat.WasPressedThisFrame())
                inputData.ValueRW.Action = InputActionType.SpawnBallCheat;
        }
    }

    private void GetInputActions(int playerIndex, out Vector2 moveInput, out bool actionPressed)
    {
        var inputActions = _inputActions.Gameplay;
        
        moveInput = Vector2.zero;
        actionPressed = false;
        
        // poll gamepads
        
        var gamepads = Gamepad.all;
        if (playerIndex < gamepads.Count)
        {
            var gamepad = gamepads[playerIndex];
            float stickInput = gamepad.leftStick.ReadValue().x;
            float dpadInput  = gamepad.dpad.x.ReadValue();
            moveInput.x = Mathf.Abs(dpadInput) > 0.01f ? dpadInput : stickInput;
            actionPressed = gamepad.buttonSouth.wasPressedThisFrame;
        }

        // add keyboard input for player 0 & 1
        
        if (playerIndex == 0)
        {
            moveInput += inputActions.P1_Move.ReadValue<Vector2>();
            actionPressed |= inputActions.P1_Action.WasPressedThisFrame();
        }
        
        if (playerIndex == 1)
        {
            moveInput += inputActions.P2_Move.ReadValue<Vector2>();
            actionPressed |= inputActions.P2_Action.WasPressedThisFrame();
        }
        
        // add mouse input for player 0 (for standalone build)
        
        if (playerIndex == 0)
        {
#if UNITY_STANDALONE
            var moveDelta = inputActions.P1_MouseMove.ReadValue<Vector2>();
            // make mouse move input frame independent dividing by Time.deltaTime
            moveInput += moveDelta / UnityEngine.Time.deltaTime / 500;
#endif
        }
        
        // add screen touch for player 0 (for mobile build)
        
#if UNITY_ANDROID || UNITY_IOS
        if (playerIndex == 0)
        {
            var pressedTouch = GetLatestPressedTouch();
            if (pressedTouch != null && !IsPointerOverGameObject(pressedTouch))
            {
                float x = pressedTouch.position.ReadValue().x;
                moveInput.x = x > Screen.width * 0.5f ? 1f : -1f;
            }
        }
#endif
    }
    
    public static void DoInputAction(EntityManager entityManager, int playerIndex, InputActionType inputAction)
    {
        using var paddlesQuery = new EntityQueryBuilder(Allocator.Temp).WithAllRW<PaddleInputData>()
            .WithAll<PlayerIndex>().Build(entityManager);
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
    
    private static bool IsPointerOverGameObject(TouchControl pressedTouch)
    {
        if (EventSystem.current == null)
            return false;
        
        if (EventSystem.current.IsPointerOverGameObject())
            return true;
        
        if (pressedTouch != null && EventSystem.current.IsPointerOverGameObject(pressedTouch.touchId.ReadValue()))
            return true;

        return false;
    }
    
    private static TouchControl GetLatestPressedTouch()
    {
        var touchscreen = Touchscreen.current;

        if (touchscreen == null)
            return null;

        TouchControl latestTouch = null;
        double latestTime = double.MinValue;

        foreach (var touch in touchscreen.touches)
        {
            if (!touch.press.isPressed)
                continue;

            double startTime = touch.startTime.ReadValue();

            if (startTime > latestTime)
            {
                latestTime = startTime;
                latestTouch = touch;
            }
        }

        return latestTouch;
    }
}
