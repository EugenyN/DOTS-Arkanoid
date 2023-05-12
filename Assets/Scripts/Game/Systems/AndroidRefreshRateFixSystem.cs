using System;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
partial class AndroidRefreshRateFixSystem : SystemBase
{
    protected override void OnCreate()
    {
        base.OnCreate();

#if UNITY_ANDROID && !UNITY_EDITOR
            // these platforms cap FPS at 30 by default, so we need to unlock it.
            // https://stackoverflow.com/questions/47031279/unity-mobile-device-30fps-locked
            Application.targetFrameRate = (int)Math.Round(Screen.currentResolution.refreshRateRatio.value);
#endif
    }

    protected override void OnUpdate()
    {
    }
}