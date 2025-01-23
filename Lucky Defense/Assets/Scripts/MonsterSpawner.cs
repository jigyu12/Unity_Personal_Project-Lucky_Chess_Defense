using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class MonsterSpawner : MonoBehaviour
{
    public GameObject monsterPrefab;

    public Tilemap monsterSpawnTilemap;

    public Vector3 monsterSpawnCellPosition;
    public List<Vector3> monsterWayCellPoint;

    public IObjectPool<Monster> monsterPool { get; private set; }
    
    private Dictionary<int, MonsterData> monsterDataDictionary { get; set; }
    
    private Vector3 cellSizeOffset;

    private void Start()
    {
        monsterPool = new ObjectPool<Monster>(OnCreateMonster, OnGetMonster, OnReleaseMonster, OnDestroyMonster);
        
        cellSizeOffset = monsterSpawnTilemap.cellSize * 0.5f;
        
        monsterSpawnCellPosition += cellSizeOffset;
        for (int i = 0; i < monsterWayCellPoint.Count; i++)
        {
            monsterWayCellPoint[i] += cellSizeOffset;
        }
        
        monsterDataDictionary = new Dictionary<int, MonsterData>();
        for (int i = 0; i < monsterDataLists.Count; i++)
        {
            List<MonsterData> monsterDataList = monsterDataLists[i].dataList;
            for (int j = 0; j < monsterDataList.Count; j++)
            {
                MonsterData monsterData = monsterDataList[j];
                if(monsterDataDictionary.ContainsKey(monsterData.Id))
                    Debug.Assert(false, "Duplicate monster ID");
                else
                    monsterDataDictionary.Add(monsterData.Id, monsterData);
            }
        }
    }
    
    void Update()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Tilemap Cell Size: " + monsterSpawnTilemap.cellSize);
            
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = monsterSpawnTilemap.WorldToCell(mouseWorldPos);

            if (monsterSpawnTilemap.HasTile(cellPosition))
            {
                Debug.Log($"Cell Position: {cellPosition}");
            }
        }
#endif
    }
    
    private Monster OnCreateMonster()
    {
        var monster = Instantiate(monsterPrefab).GetComponent<Monster>();

        monster.SetPool(monsterPool);

        return monster;
    }

    private void OnGetMonster(Monster monster)
    {
        monster.gameObject.SetActive(true);
    }

    private void OnReleaseMonster(Monster monster)
    {
        monster.gameObject.SetActive(false);
    }

    private void OnDestroyMonster(Monster monster)
    {
        Destroy(monster.gameObject);
    }

    public MonsterType SpawnMonster(int spawnMonsterId)
    {
        Monster monster = monsterPool.Get();
        
        monster.transform.position = monsterSpawnCellPosition;

        monster.waypoint = monsterWayCellPoint;
        
        bool success = SetMonsterData(monster, spawnMonsterId);
        if (success)
            return monster.monsterData.Type;
        else
        {
            Debug.Assert(false, "SetMonsterData Failed");
            
            return MonsterType.None;
        }
    }

    private bool SetMonsterData(Monster monster, int spawnMonsterId)
    {
        if (!monsterDataDictionary.ContainsKey(spawnMonsterId))
            return false;

        monster.monsterData = monsterDataDictionary[spawnMonsterId];
        
        return true;
    }
    
    [System.Serializable]
    public class MonsterDataList
    {
        public List<MonsterData> dataList = new();
    }

    [HideInInspector]
    public List<MonsterDataList> monsterDataLists = new();
    private void OnValidate()
    {
        int targetCount = (int)MonsterType.Count;

        while (monsterDataLists.Count < targetCount)
        {
            monsterDataLists.Add(new MonsterDataList());
        }

        while (monsterDataLists.Count > targetCount)
        {
            monsterDataLists.RemoveAt(monsterDataLists.Count - 1);
        }
    }
}