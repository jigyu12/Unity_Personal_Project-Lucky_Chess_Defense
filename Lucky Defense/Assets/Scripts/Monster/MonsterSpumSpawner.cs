using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class MonsterSpumSpawner : MonoBehaviour
{
    public Dictionary<int, IObjectPool<GameObject>> monsterSpumPoolDict { get; } = new();
    [SerializeField] private MonsterSpawner monsterSpawner;
    
    void Start()
    {
        monsterSpumPoolDict.Clear();

        foreach (var list in monsterSpawner.monsterDataLists)
        {
            foreach (var data in list.dataList)
            {
                IObjectPool<GameObject> spumMonsterPool = new ObjectPool<GameObject>
                    (() => OnCreateMonsterSpum(data.MonsterSpumPrefab), OnGetMonsterSpum, OnReleaseMonsterSpum, OnDestroyMonsterSpum);
                monsterSpumPoolDict.Add(data.MonsterID, spumMonsterPool);
            }
        }
    }
    
    private GameObject OnCreateMonsterSpum(GameObject monsterSpum)
    {
        return Instantiate(monsterSpum);
    }

    private void OnGetMonsterSpum(GameObject monsterSpum)
    {
        monsterSpum.SetActive(true);
    }

    private void OnReleaseMonsterSpum(GameObject monsterSpum)
    {
        monsterSpum.SetActive(false);
    }

    private void OnDestroyMonsterSpum(GameObject monsterSpum)
    {
        Destroy(monsterSpum);
    }
}