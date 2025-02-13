using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainTitleUIManager : MonoBehaviour
{
    [SerializeField] private Button gameStartButton;
    [SerializeField] private Button gameQuitButton;
    
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private Button settingPanelButton;

    public Button bgmButton;
    public Button sfxButton;
    
    [SerializeField] private List<Button> buttonList = new();
    
    private void Start()
    {
        gameStartButton.onClick.RemoveAllListeners();
        gameStartButton.onClick.AddListener(OnClickChangeSceneToInGame);
        
        gameQuitButton.onClick.RemoveAllListeners();
        gameQuitButton.onClick.AddListener(Application.Quit);
        
        settingPanelButton.onClick.RemoveAllListeners();
        settingPanelButton.onClick.AddListener(OnClickSwitchSettingPanelActive);
        
        foreach (var button in buttonList)
        {
            button.onClick.AddListener(() => SoundManager.Instance.PlaySfx(SfxClipId.UiButtonClickSfxSoundId));
        }
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