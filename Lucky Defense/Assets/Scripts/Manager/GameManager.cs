using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private InGameUIManager inGameUIManager;
    private bool isPaused;
    
    private void Awake()
    {
        Time.timeScale = 1;
        isPaused = false;
    }
    
    public void EndGame(bool isGameClear)
    {
        Time.timeScale = 0;
        
        isPaused = true;
        
        inGameUIManager.SetGameResult(isGameClear);
    }

#if UNITY_STANDALONE || UNITY_EDITOR
    
    private void Update()
    {
        if(!isPaused)
            AccelerateTime();
    }
    
    private void AccelerateTime()
    {
        if(DebugModeUI.IsDebugMode)
            Time.timeScale = 5;
        else
            Time.timeScale = 1;
    }
    
#endif
}