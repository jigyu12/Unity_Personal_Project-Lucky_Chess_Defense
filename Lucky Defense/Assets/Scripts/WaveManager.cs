using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public InGameUIManager inGameUIManager;
    public MonsterSpawner monsterSpawner;
    
    private int waveIndex;
    private float waveTimeAccumF;
    private int waveTimeAccumI;
    
    private float spawnMonsterIntervalAccum;
    private int spawnMonsterCount;

    private int currentMonsterCountToSlider;
    private const int maxMonsterCount = 100;
    
    private void Start()
    {
        waveIndex = 0;
        waveTimeAccumF = 0f;
        waveTimeAccumI = -1;

        spawnMonsterIntervalAccum = 0f;
        spawnMonsterCount = 0;

        currentMonsterCountToSlider = 0;
        
        inGameUIManager.SetWaveTimeText(waveDataList[waveIndex].WaveTime);
        inGameUIManager.SetWaveNumberText(waveDataList[waveIndex].WaveNumber);
        inGameUIManager.SetMonsterCountSliderAndText(0, maxMonsterCount);
    }

    private void Update()
    {
        CalculateWaveTime();
        
        SpawnMonsterAtInterval();
    }

    private void CalculateWaveTime()
    {
        waveTimeAccumF += Time.deltaTime;

        if (waveTimeAccumF >= 1f)
        {
            waveTimeAccumF = 0f;
            ++waveTimeAccumI;

            if (waveDataList[waveIndex].WaveTime - waveTimeAccumI >= 0)
                inGameUIManager.SetWaveTimeText(waveDataList[waveIndex].WaveTime - waveTimeAccumI);
            else
            {
                waveTimeAccumI = 0;
                spawnMonsterCount = 0;

                if (++waveIndex >= waveDataList.Count)
                    waveIndex = waveDataList.Count - 1;

                inGameUIManager.SetWaveTimeText(waveDataList[waveIndex].WaveTime);
                inGameUIManager.SetWaveNumberText(waveDataList[waveIndex].WaveNumber);
            }
        }
    }

    private void SpawnMonsterAtInterval()
    {
        spawnMonsterIntervalAccum += Time.deltaTime;

        if (spawnMonsterIntervalAccum >=
            waveDataList[waveIndex].WaveTime / (float)waveDataList[waveIndex].SpawnMonsterCount
            && spawnMonsterCount < (float)waveDataList[waveIndex].SpawnMonsterCount)
        {
            spawnMonsterCount++;
            
            spawnMonsterIntervalAccum = 0f;
            
            MonsterType currMonsterType = monsterSpawner.SpawnMonster(waveDataList[waveIndex].SpawnMonsterId);

            if (currMonsterType == MonsterType.Normal)
            {
                ++currentMonsterCountToSlider;
                
                inGameUIManager.SetMonsterCountSliderAndText(currentMonsterCountToSlider, maxMonsterCount);
            }
        }
    }

    public List<WaveData> waveDataList = new();
}