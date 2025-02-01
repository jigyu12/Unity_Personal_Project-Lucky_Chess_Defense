#if UNITY_STANDALONE || UNITY_EDITOR

using UnityEngine;

public class DebugModeUI : MonoBehaviour
{
    public static bool IsDebugMode { get; private set; }

    private void Awake()
    {
        IsDebugMode = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
            IsDebugMode = !IsDebugMode;
    }
}

#endif