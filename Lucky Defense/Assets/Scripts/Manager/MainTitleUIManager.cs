using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainTitleUIManager : MonoBehaviour
{
    [SerializeField] private Button gameStartButton;
    [SerializeField] private Button gameQuitButton;
    
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private Button settingPanelButton;
    
    private void Start()
    {
        gameStartButton.onClick.RemoveAllListeners();
        gameStartButton.onClick.AddListener(OnClickChangeSceneToInGame);
        
        gameQuitButton.onClick.RemoveAllListeners();
        gameQuitButton.onClick.AddListener(Application.Quit);
        
        settingPanelButton.onClick.RemoveAllListeners();
        settingPanelButton.onClick.AddListener(OnClickSwitchSettingPanelActive);
    }
    
    private void OnClickChangeSceneToInGame()
    {
        SceneManager.LoadScene("GameScene");
    }
    
    private void OnClickSwitchSettingPanelActive()
    {
        settingPanel.SetActive(!settingPanel.activeSelf);
    }
}