using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InGameUIManager : MonoBehaviour
{
    [FormerlySerializedAs("windowManager")] public HeroSpawner heroSpawner;
    public Button heroSpawnButton;
    
    public TMP_Text waveNumberText;
    public TMP_Text waveTimeText;

    public Slider monsterCountSlider;
    public TMP_Text monsterCountText;

    private void Start()
    {
        heroSpawnButton.onClick.AddListener(heroSpawner.OnClickCreateHero);
    }

    public void SetWaveNumberText(int waveNumber)
    {
        waveNumberText.text = $"Wave : {waveNumber}";
    }

    public void SetWaveTimeText(int waveTime)
    {
        int minutes = waveTime / 60;
        int seconds = waveTime % 60;
        
        waveTimeText.text = string.Format("{0:D2} : {1:D2}", minutes, seconds);
    }

    public void SetMonsterCountSliderAndText(int currMonsterCount, int maxMonsterCount)
    {
        monsterCountSlider.value = (float)currMonsterCount / maxMonsterCount;
        
        monsterCountText.text = $"{currMonsterCount} / {maxMonsterCount}";
    }
}