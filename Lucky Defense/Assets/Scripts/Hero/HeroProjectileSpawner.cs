using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class HeroProjectileSpawner : MonoBehaviour
{
    public Dictionary<HeroGrade, IObjectPool<GameObject>> heroProjectilePoolDict { get; } = new();
    [SerializeField] private List<GameObject> prefabList;

    void Start()
    {
        heroProjectilePoolDict.Clear();

        for(int i = 0; i < prefabList.Count; ++i)
        {
            int index = i;
            IObjectPool<GameObject> heroProjectilePool = new ObjectPool<GameObject>
                (() => OnCreateHeroProjectile(prefabList[index - 1]), OnGetHeroProjectile, OnReleaseHeroProjectile, OnDestroyHeroProjectile);
            heroProjectilePoolDict.Add((HeroGrade)i, heroProjectilePool);
        }
    }

    private GameObject OnCreateHeroProjectile(GameObject heroProjectile)
    {
        return Instantiate(heroProjectile);
    }

    private void OnGetHeroProjectile(GameObject heroProjectile)
    {
        heroProjectile.SetActive(true);
    }

    private void OnReleaseHeroProjectile(GameObject heroProjectile)
    {
        heroProjectile.SetActive(false);
    }

    private void OnDestroyHeroProjectile(GameObject heroProjectile)
    {
        Destroy(heroProjectile);
    }
}