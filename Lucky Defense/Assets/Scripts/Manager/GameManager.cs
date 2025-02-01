using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private InGameUIManager inGameUIManager;
    
    private void Awake()
    {
        Time.timeScale = 1;
    }
    
    public void EndGame(bool isGameClear)
    {
        Time.timeScale = 0;
        
        inGameUIManager.SetGameResult(isGameClear);
    }
}