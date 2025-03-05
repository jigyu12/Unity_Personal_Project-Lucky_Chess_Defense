using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using AYellowpaper.SerializedCollections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameUIManager : MonoBehaviour
{
    [SerializeField] private HeroSpawner heroSpawner;
    [SerializeField] private Button heroSpawnButton;
    
    [SerializeField] private InGameResourceManager inGameResourceManager;
    [SerializeField] private TMP_Text currCoinInButtonText;
    
    [SerializeField] private TMP_Text waveNumberText;
    [SerializeField] private TMP_Text waveTimeText;

    [SerializeField] private Slider monsterCountSlider;
    [SerializeField] private TMP_Text monsterCountText;
    
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private Button settingPanelButton;
    [SerializeField] private Button mainTitleButton;

    [SerializeField] private TMP_Text currCoinText;
    [SerializeField] private TMP_Text currGemText;
    [SerializeField] private TMP_Text heroCountText;

    [SerializeField] private GameObject gameResultPanelToUntouchable;
    [SerializeField] private GameObject gameResultPanel;
    private Image gameResultPanelImage;
    [SerializeField] private Button gameResultButton;
    [SerializeField] private TMP_Text gameResultText;
    
    [SerializeField] private Button probabilityInfoButton;
    [SerializeField] private GameObject probabilityInfoPanel;
    
    [SerializeField] private Button probabilityEnforceButton;
    [SerializeField] private TMP_Text probabilityEnforceText;
    [SerializeField] private TMP_Text currCoinInProbabilityButtonText;
    [SerializeField] private TMP_Text commonProbabilityText;
    [SerializeField] private TMP_Text rareProbabilityText;
    [SerializeField] private TMP_Text heroicProbabilityText;
    [SerializeField] private TMP_Text legendaryProbabilityText;

    [SerializeField] private Button luckySummonButton;
    [SerializeField] private GameObject luckySummonPanel;
    [SerializeField] private Button luckySummonCancelButton;
    [SerializeField] private TMP_Text rareSummonCostButtonText;
    [SerializeField] private TMP_Text heroicSummonCostButtonText;
    [SerializeField] private TMP_Text legendarySummonCostButtonText;
    [SerializeField] private Button rareSummonButton;
    [SerializeField] private Button heroicSummonButton;
    [SerializeField] private Button legendarySummonButton;

    [SerializeField] private List<TMP_Text> logTextLineList;
    private int logTextStringQueueMaxSize;
    private Queue<String> logTextStringQueue = new();
    
    private readonly StringBuilder stringBuilder = new(20);

    public List<Sprite> inGameResourceIcons;
    
    public Button bgmButton;
    public Button sfxButton;
    public List<Image> soundImageList;
    
    [SerializeField] private List<Button> buttonList = new();
    
    [SerializeField] private Button synergyButton;
    [SerializeField] private Button synergyPanelCloseButton;
    [SerializeField] private GameObject synergyPanel;
    
    [SerializeField] private List<Button> allySynergyButtonList;
    [SerializeField] private List<GameObject> allySynergyPanelList;
    [SerializeField] private List<TMP_Text> knightsSynergyTextList;
    [SerializeField] private List<TMP_Text> thievesSynergyTextList;
    [SerializeField] private List<TMP_Text> religiousSynergyTextList;
    private readonly Color inActiveSynergyColor = new Color(255f, 255f, 255f, 0.4f);
    private readonly Color activeSynergyColor = new Color(255f, 255f, 255f, 1f);

    [SerializeField] private GameObject heroInfoPanel;
    private HeroSpawnPointInCell clickedCell;
    
    [SerializedDictionary("Hero Id","Hero Image")]
    [SerializeField] private SerializedDictionary<int, Sprite> heroImageDict;
    [SerializeField] private Image heroImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text gradeText;
    [SerializeField] private TMP_Text synergyText;
    [SerializeField] private TMP_Text attackTypeText;
    [SerializeField] private TMP_Text attackDamageText;
    [SerializeField] private TMP_Text attackSpeedText;
    [SerializeField] private TMP_Text criticalRateText;
    [SerializeField] private TMP_Text criticalDamageText;
    [SerializeField] private Image synergyImage;
    [SerializeField] private Image attackTypeImage;
    [SerializedDictionary("Synergy","Synergy Image")]
    [SerializeField] private SerializedDictionary<SynergyClass, Sprite> synergyImageDict;
    [SerializedDictionary("Attack Type","Attack Type Image")]
    [SerializeField] private SerializedDictionary<HeroAttackType, Sprite> attackTypeImageDict;
    
    private void Awake()
    {
        stringBuilder.Clear();
        
        logTextStringQueue.Clear();

        logTextStringQueueMaxSize = logTextLineList.Count;

        foreach (var logTextLine in logTextLineList)
        {
            stringBuilder.Clear();

            stringBuilder.Append(" ");
            
            logTextLine.SetText(stringBuilder.ToString());
        }

        clickedCell = null;
    }

    private void Start()
    {
        heroSpawnButton.onClick.RemoveAllListeners();
        heroSpawnButton.onClick.AddListener(() => heroSpawner.OnClickCreateHero());
        heroSpawnButton.onClick.AddListener(SetCurrCoinInButtonText);
        
        settingPanelButton.onClick.RemoveAllListeners();
        settingPanelButton.onClick.AddListener(OnClickSwitchSettingPanelActive);
        
        mainTitleButton.onClick.RemoveAllListeners();
        mainTitleButton.onClick.AddListener(OnClickChangeSceneToMainTitle);

        gameResultPanel.TryGetComponent(out gameResultPanelImage);
        gameResultButton.onClick.RemoveAllListeners();
        gameResultButton.onClick.AddListener(OnClickChangeSceneToMainTitle);
        
        probabilityInfoButton.onClick.RemoveAllListeners();
        probabilityInfoButton.onClick.AddListener(OnClickSwitchProbabilityInfoPanelActive);
        
        probabilityEnforceButton.onClick.RemoveAllListeners();
        probabilityEnforceButton.onClick.AddListener(heroSpawner.OnClickEnforceProbability);
        
        commonProbabilityText.color = Color.white;
        rareProbabilityText.color = Color.blue;
        heroicProbabilityText.color = new Color(0.65f, 0.32f, 0.92f);
        legendaryProbabilityText.color = Color.yellow;
        
        luckySummonButton.onClick.RemoveAllListeners();
        luckySummonButton.onClick.AddListener(OnClickLuckySummonPanelActive);
        
        luckySummonCancelButton.onClick.RemoveAllListeners();
        luckySummonCancelButton.onClick.AddListener(OnClickLuckySummonPanelInActive);
        
        rareSummonButton.onClick.RemoveAllListeners();
        rareSummonButton.onClick.AddListener(() => 
            heroSpawner.OnClickCreateHero(true, heroSpawner.RareSummonOnlyProbability, HeroGrade.Rare,
                true,true,false));
        
        heroicSummonButton.onClick.RemoveAllListeners();
        heroicSummonButton.onClick.AddListener(() => 
            heroSpawner.OnClickCreateHero(true, heroSpawner.HeroicSummonOnlyProbability, HeroGrade.Heroic,
                true,true ,false));
        
        legendarySummonButton.onClick.RemoveAllListeners();
        legendarySummonButton.onClick.AddListener(() => 
            heroSpawner.OnClickCreateHero(true, heroSpawner.LegendarySummonOnlyProbability, HeroGrade.Legendary,
                true,true,false));
        
        synergyButton.onClick.RemoveAllListeners();
        synergyButton.onClick.AddListener(OnClickSwitchSynergyPanelActive);
        
        synergyPanelCloseButton.onClick.RemoveAllListeners();
        synergyPanelCloseButton.onClick.AddListener(OnClickSynergyPanelInactive);

        for (int i = 0; i < allySynergyButtonList.Count; i++)
        {
            int index = i;
            
            allySynergyButtonList[i].onClick.RemoveAllListeners();
            allySynergyButtonList[i].onClick.AddListener(() => OnClickSwitchAllySynergyPanelActive(index));
        }
        
        foreach (var button in buttonList)
        {
            button.onClick.AddListener(() => SoundManager.Instance.PlaySfx(SfxClipId.UiButtonClickSfxSoundId));
        }
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

    public void SetCurrCoinText(int currCoin)
    {
        stringBuilder.Clear();
        
        stringBuilder.Append(currCoin.ToString());
        
        currCoinText.SetText(stringBuilder.ToString());
    }

    public void SetCurrGemText(int currGem)
    {
        stringBuilder.Clear();
        
        stringBuilder.Append(currGem.ToString());
        
        currGemText.SetText(stringBuilder.ToString());
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

    public void SetGameResultPanel(bool isGameClear)
    {
        stringBuilder.Clear();
        
        gameResultPanelToUntouchable.SetActive(true);
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

    private void SetCurrCoinInButtonText()
    {
        stringBuilder.Clear();

        stringBuilder.Append(inGameResourceManager.CurrentHeroSummonCoinCost.ToString());

        currCoinInButtonText.SetText(stringBuilder.ToString());
    }
    
    private void OnClickSwitchProbabilityInfoPanelActive()
    {
        probabilityInfoPanel.SetActive(!probabilityInfoPanel.activeSelf);
        luckySummonButton.gameObject.SetActive(!luckySummonButton.gameObject.activeSelf);
    }

    public void SetProbabilityTexts()
    {
        var currProbData = heroSpawner.CurrentHeroSummonProbabilityData;
        
        {
            stringBuilder.Clear();

            stringBuilder.AppendFormat($"Probability Enforce Lv {heroSpawner.CurrProbabilityLevel}");
        
            probabilityEnforceText.SetText(stringBuilder.ToString());
        }
        {
            stringBuilder.Clear();
            
            stringBuilder.Append(currProbData.EnforceCost.ToString());
            
            currCoinInProbabilityButtonText.SetText(stringBuilder.ToString());
        }
        {
            stringBuilder.Clear();
            
            stringBuilder.AppendFormat($"Common \n: {currProbData.CommonProbability:F2} %");
            
            commonProbabilityText.SetText(stringBuilder.ToString());
        }
        {
            stringBuilder.Clear();
            
            stringBuilder.AppendFormat($"Rare \n: {currProbData.RareProbability:F2} %");
            
            rareProbabilityText.SetText(stringBuilder.ToString());
        }
        {
            stringBuilder.Clear();
            
            stringBuilder.AppendFormat($"Heroic \n: {currProbData.HeroicProbability:F2} %");
            
            heroicProbabilityText.SetText(stringBuilder.ToString());
        }
        {
            stringBuilder.Clear();
            
            stringBuilder.AppendFormat($"Legendary \n: {currProbData.LegendaryProbability:F2} %");
            
            legendaryProbabilityText.SetText(stringBuilder.ToString());
        }
    }

    private void OnClickLuckySummonPanelActive()
    {
        luckySummonPanel.SetActive(true);
        luckySummonCancelButton.gameObject.SetActive(true);
    }

    private void OnClickLuckySummonPanelInActive()
    {
        luckySummonPanel.SetActive(false);
        luckySummonCancelButton.gameObject.SetActive(false);
    }

    public void SetLuckySummonGemCostTexts()
    {
        {
            stringBuilder.Clear();

            stringBuilder.Append(inGameResourceManager.InitialRareSummonGemCost.ToString());
        
            rareSummonCostButtonText.SetText(stringBuilder.ToString());
        }
        {
            stringBuilder.Clear();
            
            stringBuilder.Append(inGameResourceManager.InitialHeroicSummonGemCost.ToString());
            
            heroicSummonCostButtonText.SetText(stringBuilder.ToString());
        }
        {
            stringBuilder.Clear();
            
            stringBuilder.Append(inGameResourceManager.InitialLegendarySummonGemCost.ToString());
            
            legendarySummonCostButtonText.SetText(stringBuilder.ToString());
        }
    }

    public void SetLogText(string logText)
    {
        if (logTextStringQueue.Count > logTextStringQueueMaxSize)
        {
            Debug.Assert(false, "Invalid logTextStringQueue operation.");
            return;
        }

        if (logTextStringQueue.Count == logTextStringQueueMaxSize)
            logTextStringQueue.Dequeue();
    
        logTextStringQueue.Enqueue(logText);

        var logs = logTextStringQueue.ToArray();
        int index = 0;
        for (int i = logs.Length - 1; i >= 0; i--)
        {
            stringBuilder.Clear();
            stringBuilder.Append(logs[i]);
        
            logTextLineList[index++].SetText(stringBuilder.ToString());
        }
    }

    private void OnClickSwitchSynergyPanelActive()
    {
        synergyPanel.SetActive(!synergyPanel.activeSelf);
    }
    
    private void OnClickSynergyPanelInactive()
    {
        synergyPanel.SetActive(false);
    }

    private void OnClickSwitchAllySynergyPanelActive(int index)
    {
        for (int i = 0; i < allySynergyPanelList.Count; ++i)
        {
            if (i < 0 || i >= allySynergyPanelList.Count)
            {
                Debug.Assert(false,"Invalid index operation in active AllySynergyPanel.");
            }
            
            if(i == index)
                allySynergyPanelList[i].SetActive(true);
            else
                allySynergyPanelList[i].SetActive(false);
        }
    }

    public void SetAllySynergyTextActive(SynergyClass synergyClass, int index)
    {
        switch (synergyClass)
        {
            case SynergyClass.Knights:
            {
                for (int i = 0; i < knightsSynergyTextList.Count; ++i)
                {
                    if (index == -1)
                    {
                        knightsSynergyTextList[i].color = inActiveSynergyColor;
                        
                        continue;
                    }
                    
                    if (i == index)
                    {
                        knightsSynergyTextList[i].color = activeSynergyColor;
                    }
                    else
                    {
                        knightsSynergyTextList[i].color = inActiveSynergyColor;
                    }
                }
            }
                break;
            case SynergyClass.Thieves:
            {
                for (int i = 0; i < thievesSynergyTextList.Count; ++i)
                {
                    if (index == -1)
                    {
                        thievesSynergyTextList[i].color = inActiveSynergyColor;
                        
                        continue;
                    }
                    
                    if (i == index)
                    {
                        thievesSynergyTextList[i].color = activeSynergyColor;
                    }
                    else
                    {
                        thievesSynergyTextList[i].color = inActiveSynergyColor;
                    }
                }
            }
                break;
            case SynergyClass.Religious:
            {
                for (int i = 0; i < religiousSynergyTextList.Count; ++i)
                {
                    if (index == -1)
                    {
                        religiousSynergyTextList[i].color = inActiveSynergyColor;
                        
                        continue;
                    }
                    
                    if (i == index)
                    {
                        religiousSynergyTextList[i].color = activeSynergyColor;
                    }
                    else
                    {
                        religiousSynergyTextList[i].color = inActiveSynergyColor;
                    }
                }
            }
                break;
        }
    }

    public void SetHeroInfoPanelActive(HeroSpawnPointInCell cell, HeroInfoData heroInfoData)
    {
        clickedCell = cell;

        if (heroImageDict.TryGetValue(heroInfoData.HeroID, out _))
            heroImage.sprite = heroImageDict[heroInfoData.HeroID];

        nameText.SetText(heroInfoData.HeroID.ToString());
        gradeText.SetText(heroInfoData.HeroGrade.ToString());
        synergyText.SetText(((SynergyClass)heroInfoData.HeroSynergyClass).ToString());
        attackTypeText.SetText(heroInfoData.AtkType.ToString());
        attackDamageText.SetText(heroInfoData.AtkDamage.ToString());
        attackSpeedText.SetText(heroInfoData.AtkSpeed.ToString());
        criticalRateText.SetText(heroInfoData.CriticalRate.ToString());
        criticalDamageText.SetText(heroInfoData.CriticalMlt.ToString());
        
        if (synergyImageDict.TryGetValue((SynergyClass)heroInfoData.HeroSynergyClass, out _))
            synergyImage.sprite = synergyImageDict[(SynergyClass)heroInfoData.HeroSynergyClass];
        
        if (attackTypeImageDict.TryGetValue(heroInfoData.AtkType, out _))
            attackTypeImage.sprite = attackTypeImageDict[heroInfoData.AtkType];
        
        heroInfoPanel.SetActive(true);
    }

    public void SetHeroInfoPanelInactive(HeroSpawnPointInCell cell)
    {
        if(clickedCell == cell)
            heroInfoPanel.SetActive(false);
    }
}