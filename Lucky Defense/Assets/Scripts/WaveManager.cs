using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private InGameUIManager inGameUIManager;
    [SerializeField] private MonsterSpawner monsterSpawner;
    
    private int currWaveIndex;
    private float waveTimeAccumF;
    private int waveTimeAccumI;
    
    private float spawnMonsterIntervalAccum;
    private float spawnMonsterIntervalDelay;
    private int spawnMonsterCount;
    
    public int CurrentMonsterCountToSlider { get; set; }
    public int MaxMonsterCount => 100;
    public float CurrHpMultiplier => waveDataList[currWaveIndex].HpMultiplier;

    public Dictionary<Collider2D, Monster> CurrMonstersDict { get; } = new();

    private void Awake()
    {
        currWaveIndex = 0;
        waveTimeAccumF = 0f;
        waveTimeAccumI = -1;
        
        spawnMonsterIntervalDelay =  waveDataList[currWaveIndex].WaveTime / (float)waveDataList[currWaveIndex].SpawnMonsterCount;
        spawnMonsterIntervalAccum = spawnMonsterIntervalDelay;

        spawnMonsterCount = 0;

        CurrentMonsterCountToSlider = 0;
        
        foreach (var pair in CurrMonstersDict)
            pair.Value.DestroyMonster();
        
        CurrMonstersDict.Clear();
    }

    private void Start()
    {
        inGameUIManager.SetWaveTimeText(waveDataList[currWaveIndex].WaveTime);
        inGameUIManager.SetWaveNumberText(waveDataList[currWaveIndex].WaveNumber);
        inGameUIManager.SetMonsterCountSliderAndText(0, MaxMonsterCount);
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        
        CalculateWaveTime(deltaTime);
        
        SpawnMonsterAtInterval(deltaTime);
    }

    private void CalculateWaveTime(float deltaTime)
    {
        waveTimeAccumF += deltaTime;

        if (waveTimeAccumF >= 1f)
        {
            waveTimeAccumF = 0f;
            ++waveTimeAccumI;

            int remainWaveTime = waveDataList[currWaveIndex].WaveTime - waveTimeAccumI;
            if (remainWaveTime >= 0)
                inGameUIManager.SetWaveTimeText(remainWaveTime);
            else
            {
                waveTimeAccumI = 0;
                spawnMonsterCount = 0;

                if (++currWaveIndex >= waveDataList.Count)
                    currWaveIndex = waveDataList.Count - 1;
                
                spawnMonsterIntervalDelay =  waveDataList[currWaveIndex].WaveTime / (float)waveDataList[currWaveIndex].SpawnMonsterCount;
                spawnMonsterIntervalAccum = spawnMonsterIntervalDelay;

                inGameUIManager.SetWaveTimeText(waveDataList[currWaveIndex].WaveTime);
                inGameUIManager.SetWaveNumberText(waveDataList[currWaveIndex].WaveNumber);
            }
        }
    }

    private void SpawnMonsterAtInterval(float deltaTime)
    {
        spawnMonsterIntervalAccum += deltaTime;

        if (spawnMonsterIntervalAccum >= spawnMonsterIntervalDelay
            && spawnMonsterCount < (float)waveDataList[currWaveIndex].SpawnMonsterCount)
        {
            spawnMonsterCount++;
            
            spawnMonsterIntervalAccum = 0f;
            
            Monster monster = monsterSpawner.SpawnMonster(waveDataList[currWaveIndex].SpawnMonsterId);
            
            if (monster is null)
                return;

            CurrMonstersDict.Add(monster.Coll2D, monster);

            if (monster.Type == MonsterType.Normal)
            {
                ++CurrentMonsterCountToSlider;
                
                inGameUIManager.SetMonsterCountSliderAndText(CurrentMonsterCountToSlider, MaxMonsterCount);
            }
        }
    }

    public List<WaveData> waveDataList = new();
}