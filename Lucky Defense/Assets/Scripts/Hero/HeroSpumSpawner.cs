using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class HeroSpumSpawner : MonoBehaviour
{
    public Dictionary<int, IObjectPool<GameObject>> heroSpumPoolDict { get; } = new();
    [SerializeField] private HeroSpawner heroSpawner;
    
    void Start()
    {
        heroSpumPoolDict.Clear();

        foreach (var list in heroSpawner.heroDataRarityLists)
        {
            foreach (var data in list.dataList)
            {
                IObjectPool<GameObject> spumHeroPool = new ObjectPool<GameObject>
                    (() => OnCreateHeroSpum(data.HeroSpumPrefab), OnGetHeroSpum, OnReleaseHeroSpum, OnDestroyHeroSpum);
                heroSpumPoolDict.Add(data.HeroID, spumHeroPool);
            }
        }
    }
    
    private GameObject OnCreateHeroSpum(GameObject heroSpum)
    {
        return Instantiate(heroSpum);
    }

    private void OnGetHeroSpum(GameObject heroSpum)
    {
        heroSpum.SetActive(true);
    }

    private void OnReleaseHeroSpum(GameObject heroSpum)
    {
        heroSpum.SetActive(false);
    }

    private void OnDestroyHeroSpum(GameObject heroSpum)
    {
        Destroy(heroSpum);
    }
}