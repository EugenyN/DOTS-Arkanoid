using UnityEngine;

public class Unity2021AndroidRefreshRateFix : MonoBehaviour
{
#if UNITY_ANDROID && UNITY_2021 && !UNITY_EDITOR 
    private void Start()
    {
        // fix for > 30 fps refresh rate on android device
        // https://stackoverflow.com/questions/47031279/unity-mobile-device-30fps-locked
        Application.targetFrameRate = 120;
    }
#endif
}