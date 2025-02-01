using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private InGameUIManager inGameUIManager;
    [SerializeField] private MonsterSpawner monsterSpawner;
    [SerializeField] private GameManager gameManager;
    
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
    
    private const int BossWaveDivider = 3; // original value : 10
    private const int LastBossWaveRemainder = 1; // original value : 8
    private int bossMonsterCount;

    private void Awake()
    {
        currWaveIndex = 0;
        waveTimeAccumF = 0f;
        waveTimeAccumI = 0;
        
        spawnMonsterIntervalDelay =  waveDataList[currWaveIndex].WaveTime / (float)waveDataList[currWaveIndex].SpawnMonsterCount;
        spawnMonsterIntervalAccum = spawnMonsterIntervalDelay;

        spawnMonsterCount = 0;

        CurrentMonsterCountToSlider = 0;
        
        foreach (var pair in CurrMonstersDict)
            pair.Value.DestroyMonster();
        
        CurrMonstersDict.Clear();

        bossMonsterCount = 0;
    }

    private void Start()
    {
        inGameUIManager.SetWaveTimeText(waveDataList[currWaveIndex].WaveTime);
        inGameUIManager.SetWaveNumberText(waveDataList[currWaveIndex].WaveNumber);
        inGameUIManager.SetMonsterCountSliderAndText(CurrentMonsterCountToSlider, MaxMonsterCount);
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

                if (waveDataList[currWaveIndex].WaveNumber % BossWaveDivider == 0 && bossMonsterCount > 0)
                {
                    gameManager.EndGame(false);

                    return;
                }
                
                if (++currWaveIndex >= waveDataList.Count)
                    currWaveIndex = waveDataList.Count - 1;
                
                if (waveDataList[currWaveIndex].WaveNumber % BossWaveDivider == 0)
                    bossMonsterCount = waveDataList[currWaveIndex].SpawnMonsterCount;
                
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
            ++spawnMonsterCount;
            
            spawnMonsterIntervalAccum = 0f;
            
            Monster monster = monsterSpawner.SpawnMonster(waveDataList[currWaveIndex].SpawnMonsterId);
            
            if (monster is null)
                return;

            CurrMonstersDict.Add(monster.Coll2D, monster);

            if (monster.Type == MonsterType.Normal)
                inGameUIManager.SetMonsterCountSliderAndText(++CurrentMonsterCountToSlider, MaxMonsterCount);

            if (CurrentMonsterCountToSlider >= MaxMonsterCount)
                gameManager.EndGame(false);
        }
    }

    public void ReduceBossMonsterCount()
    {
        if (--bossMonsterCount <= 0 && waveDataList[currWaveIndex].WaveNumber / BossWaveDivider == LastBossWaveRemainder)
            gameManager.EndGame(true);
    }

    public List<WaveData> waveDataList = new();
}