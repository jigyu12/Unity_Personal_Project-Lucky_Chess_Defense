using UnityEngine;
using UnityEngine.UI;

public class FrameCounter : MonoBehaviour
{
    private float deltaTime;

    [SerializeField] private int size = 100;
    [SerializeField] private Color color = Color.red;
    
    [SerializeField] private Button frameCounterButton;
    
    private bool isVisibleFrame;

    void Awake()
    {
        deltaTime = 0f;
        
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 9999;

        isVisibleFrame = false;
    }

    private void Start()
    {
        frameCounterButton.onClick.AddListener(OnClickSwitchFrameVisible);
    }

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    private void OnGUI()
    {
        if(!isVisibleFrame)
            return;
        
        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(30, 30, Screen.width, Screen.height - 200);
        style.alignment = TextAnchor.LowerCenter;
        style.fontSize = size;
        style.normal.textColor = color;

        float ms = deltaTime * 1000f;
        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.} FPS ({1:0.0} ms)", fps, ms);

        GUI.Label(rect, text, style);
    }

    private void OnClickSwitchFrameVisible()
    {
        isVisibleFrame = !isVisibleFrame;
    }
}