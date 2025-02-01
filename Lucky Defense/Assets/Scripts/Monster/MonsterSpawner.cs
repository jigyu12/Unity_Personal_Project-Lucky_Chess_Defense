using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Tilemaps;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private GameObject monsterPrefab;

    [SerializeField] private Tilemap monsterSpawnTilemap;

    [SerializeField] private Vector3 monsterSpawnCellPosition;
    [SerializeField] private List<Vector3> monsterWayCellPoint;

    private IObjectPool<Monster> MonsterPool { get; set; }

    private Dictionary<int, MonsterData> MonsterDataDict { get; } = new();
    
    private Vector3 cellSizeOffset;

    private void Awake()
    {
        MonsterPool = new ObjectPool<Monster>(OnCreateMonster, OnGetMonster, OnReleaseMonster, OnDestroyMonster);
        
        MonsterDataDict.Clear();
        foreach (var monsterDataList in monsterDataLists)
        {
            List<MonsterData> dataList = monsterDataList.dataList;
            foreach (var monsterData in dataList)
            {
                if(!MonsterDataDict.TryAdd(monsterData.Id, monsterData))
                    Debug.Assert(false, $"Duplicate monster ID {monsterData.Id.ToString()}");
            }
        }
    }

    private void Start()
    {
        cellSizeOffset = monsterSpawnTilemap.cellSize * 0.5f;
        
        monsterSpawnCellPosition += cellSizeOffset;
        for (int i = 0; i < monsterWayCellPoint.Count; ++i)
        {
            monsterWayCellPoint[i] += cellSizeOffset;
        }
    }
    
    private Monster OnCreateMonster()
    {
        Instantiate(monsterPrefab).TryGetComponent(out Monster monster);

        monster.SetPool(MonsterPool);

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

    public Monster SpawnMonster(int spawnMonsterId)
    {
        Monster monster = MonsterPool.Get();
        
        bool success = SetMonsterData(monster, spawnMonsterId);
        
        if (success)
        {
            monster.transform.position = monsterSpawnCellPosition;

            monster.SetWaypoint(monsterWayCellPoint);
            
            monster.Initialize();

            return monster;
        }
        else
        {
            MonsterPool.Release(monster);
            
            Debug.Assert(false, "SetMonsterData Failed");
            
            return null;
        }
    }

    private bool SetMonsterData(Monster monster, int spawnMonsterId)
    {
        if (!MonsterDataDict.TryGetValue(spawnMonsterId, out MonsterData monsterData))
            return false;

        monster.SetMonsterData(monsterData);
        
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
    
    // Get Tilemap cellSize by Click MouseLeftButton
    // if (Input.GetMouseButtonDown(0))
    // {
    //     Debug.Log("Tilemap Cell Size: " + monsterSpawnTilemap.cellSize);
    //     
    //     Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //     Vector3Int cellPosition = monsterSpawnTilemap.WorldToCell(mouseWorldPos);
    //
    //     if (monsterSpawnTilemap.HasTile(cellPosition))
    //     {
    //         Debug.Log($"Cell Position: {cellPosition}");
    //     }
    // }
}