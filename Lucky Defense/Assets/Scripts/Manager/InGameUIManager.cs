using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameUIManager : MonoBehaviour
{
    [SerializeField] private HeroSpawner heroSpawner;
    [SerializeField] private Button heroSpawnButton;
    
    [SerializeField] private TMP_Text waveNumberText;
    [SerializeField] private TMP_Text waveTimeText;

    [SerializeField] private Slider monsterCountSlider;
    [SerializeField] private TMP_Text monsterCountText;
    
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private Button settingPanelButton;
    [SerializeField] private Button mainTitleButton;

    [SerializeField] private TMP_Text heroCountText;
    
    [SerializeField] private GameObject gameResultPanel;
    private Image gameResultPanelImage;
    [SerializeField] private Button gameResultButton;
    [SerializeField] private TMP_Text gameResultText;

    private readonly StringBuilder stringBuilder = new(20);

    private void Awake()
    {
        stringBuilder.Clear();
    }

    private void Start()
    {
        heroSpawnButton.onClick.RemoveAllListeners();
        heroSpawnButton.onClick.AddListener(heroSpawner.OnClickCreateHero);
        
        settingPanelButton.onClick.RemoveAllListeners();
        settingPanelButton.onClick.AddListener(OnClickSwitchSettingPanelActive);
        
        mainTitleButton.onClick.RemoveAllListeners();
        mainTitleButton.onClick.AddListener(OnClickChangeSceneToMainTitle);

        gameResultPanel.TryGetComponent(out gameResultPanelImage);
        gameResultButton.onClick.RemoveAllListeners();
        gameResultButton.onClick.AddListener(OnClickChangeSceneToMainTitle);
    }

    public void SetWaveNumberText(int waveNumber)
    {
        stringBuilder.Clear();
        
        stringBuilder.AppendFormat("Wave : {0}", waveNumber.ToString());
        
        waveNumberText.SetText(stringBuilder.ToString());
    }

    public void SetWaveTimeText(int waveTime)
    {
        int minutes = waveTime / 60;
        int seconds = waveTime % 60;
        
        stringBuilder.Clear();
        
        stringBuilder.AppendFormat("{0} : {1}", minutes.ToString("D2"), seconds.ToString("D2"));
        
        waveTimeText.SetText(stringBuilder.ToString());
    }

    public void SetMonsterCountSliderAndText(int currMonsterCount, int maxMonsterCount)
    {
        monsterCountSlider.value = (float)currMonsterCount / maxMonsterCount;
        
        stringBuilder.Clear();
        
        stringBuilder.AppendFormat("{0} / {1}", currMonsterCount.ToString(), maxMonsterCount.ToString());
        
        monsterCountText.SetText(stringBuilder.ToString());
    }

    public void SetHeroCountText(int currHeroCount, int maxHeroCount)
    {
        stringBuilder.Clear();
        
        stringBuilder.AppendFormat("{0} / {1}", currHeroCount.ToString("D2"), maxHeroCount.ToString("D2"));
        
        heroCountText.SetText(stringBuilder.ToString());
    }

    private void OnClickSwitchSettingPanelActive()
    {
        settingPanel.SetActive(!settingPanel.activeSelf);
    }

    private void OnClickChangeSceneToMainTitle()
    {
        SceneManager.LoadScene("MainTitleScene");
    }

    public void SetGameResult(bool isGameClear)
    {
        stringBuilder.Clear();
        
        gameResultPanel.SetActive(true);

        if (isGameClear)
        {
            gameResultPanelImage.color = Color.green;

            stringBuilder.Append("Game Clear!");
            gameResultText.SetText(stringBuilder.ToString());
        }
        else
        {
            gameResultPanelImage.color = Color.red;
            
            stringBuilder.Append("Game Over...");
            gameResultText.SetText(stringBuilder.ToString());
        }
    }
}