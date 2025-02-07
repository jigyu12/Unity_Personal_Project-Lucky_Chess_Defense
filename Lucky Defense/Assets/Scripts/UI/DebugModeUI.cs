#if UNITY_STANDALONE || UNITY_EDITOR

using UnityEngine;
using UnityEngine.UI;

public class DebugModeUI : MonoBehaviour
{
    public static bool IsDebugMode { get; private set; }
    [SerializeField] private Button debugButton;
    [SerializeField] private InGameResourceManager inGameResourceManager;

    private void Awake()
    {
        IsDebugMode = false;
        
        debugButton.onClick.RemoveAllListeners();
        debugButton.onClick.AddListener(OnClickDebugMode);
        debugButton.onClick.AddListener(inGameResourceManager.OnClickShowMeTheMoney);
    }

    private void OnClickDebugMode()
    {
        IsDebugMode = !IsDebugMode;
    }
}

#endif