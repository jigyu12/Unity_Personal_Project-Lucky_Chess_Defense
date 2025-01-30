using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUIManager : MonoBehaviour
{
    [SerializeField] private HeroSpawner heroSpawner;
    [SerializeField] private Button heroSpawnButton;
    
    [SerializeField] private TMP_Text waveNumberText;
    [SerializeField] private TMP_Text waveTimeText;

    [SerializeField] private Slider monsterCountSlider;
    [SerializeField] private TMP_Text monsterCountText;

    private void Start()
    {
        heroSpawnButton.onClick.RemoveAllListeners();
        heroSpawnButton.onClick.AddListener(heroSpawner.OnClickCreateHero);
    }

    public void SetWaveNumberText(int waveNumber)
    {
        waveNumberText.SetText($"Wave : {waveNumber}");
    }

    public void SetWaveTimeText(int waveTime)
    {
        int minutes = waveTime / 60;
        int seconds = waveTime % 60;
        
        waveTimeText.SetText($"{minutes:D2} : {seconds:D2}");
    }

    public void SetMonsterCountSliderAndText(int currMonsterCount, int maxMonsterCount)
    {
        monsterCountSlider.value = (float)currMonsterCount / maxMonsterCount;
        
        monsterCountText.SetText($"{currMonsterCount} / {maxMonsterCount}");
    }
}