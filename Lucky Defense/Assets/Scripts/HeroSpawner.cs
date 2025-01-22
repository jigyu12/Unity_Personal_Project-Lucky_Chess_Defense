using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class HeroSpawner : MonoBehaviour
{
    public Button heroSummonButton;
    
    public GameObject heroPrefab;
    public GameObject heroSpawnPointInCellPrefab;

    public Tilemap heroSpawnTilemap;
    public IObjectPool<Hero> heroPool { get; private set; }

    public int HeroSummonProbabilityIndex { get; set; }
    
    private List<Vector3Int> heroSpawnPositionList;
    private List<HeroSpawnPointInCell> heroSpawnPointInCellList;
    
    private void Start()
    {
        RectTransform heroSummonButtonRectTr = heroSummonButton.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToWorldPointInRectangle(heroSummonButtonRectTr, heroSummonButtonRectTr.position, Camera.main, out Vector3 position);
        transform.position = position;

        heroPool = new ObjectPool<Hero>(OnCreateHero, OnGetHero, OnReleaseHero, OnDestroyHero);
        
        heroSummonButton.onClick.AddListener(OnClickCreateHero);
        
        heroSpawnPositionList = new();
        heroSpawnPointInCellList = new();
        
        BoundsInt bounds = heroSpawnTilemap.cellBounds;
        
        foreach (Vector3Int cellPos in bounds.allPositionsWithin)
        {
            if (heroSpawnTilemap.HasTile(cellPos))
            {
                heroSpawnPositionList.Add(cellPos);
            }
        }
        
        heroSpawnPositionList.Sort((a, b) =>
        {
            if (a.y != b.y)
            {
                return b.y.CompareTo(a.y);
            }
            return a.x.CompareTo(b.x);
        });

        for (int i = 0; i < heroSpawnPositionList.Count; i++)
        {
            HeroSpawnPointInCell cell = Instantiate(heroSpawnPointInCellPrefab).GetComponent<HeroSpawnPointInCell>();
            heroSpawnPointInCellList.Add(cell);
            
            cell.transform.position = heroSpawnPositionList[i];
        }

        HeroSummonProbabilityIndex = 0;
    }

    private void OnClickCreateHero()
    {
        Hero hero = heroPool.Get();

        bool success = SetHeroDataByRandData(hero);
        Debug.Assert(success, "SetHeroData Failed");
        Debug.Log(hero.heroData.HeroId);

        for (int i = 0; i < heroSpawnPointInCellList.Count; i++)
        {
            if(heroSpawnPointInCellList[i].CanSpawnHero(hero))
                return;
        }
        
        Debug.Log("Cant Spawn Hero");
        hero.DestroyHero();
    }

    private bool SetHeroDataByRandData(Hero hero)
    {
        if(HeroSummonProbabilityIndex < 0 || HeroSummonProbabilityIndex >= heroSummonProbabilityDataLists.Count)
            return false;

        HeroSummonProbabilityData pData = heroSummonProbabilityDataLists[HeroSummonProbabilityIndex];
        
        List<float> probabilities = new();
        foreach (var property in typeof(HeroSummonProbabilityData).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.PropertyType == typeof(float) && property.Name.EndsWith("Probability"))
            {
                float value = (float)property.GetValue(pData);
                probabilities.Add(value);
            }
        }
        
        float probability = Random.value * 100f;
        float pSum = 0f;
        for (int i = 0; i < (int)HeroRarity.Count; i++)
        {
            pSum += probabilities[i];

            if (probability <= pSum)
            {
                return SetHeroData(hero, i);
            }
        }
        
        return false;
    }

    private bool SetHeroData(Hero hero, int listIndex)
    {
        if(listIndex < 0 || listIndex >= heroDataRarityLists.Count)
            return false;

        var dataList = heroDataRarityLists[listIndex].dataList;
        hero.heroData = dataList[Random.Range(0, dataList.Count)];
        
        return true;
    }
    
    private Hero OnCreateHero()
    {
        var hero = Instantiate(heroPrefab).GetComponent<Hero>();
        
        hero.SetPool(heroPool);
        
        return hero;
    }

    private void OnGetHero(Hero hero)
    {
        hero.gameObject.SetActive(true);
    }

    private void OnReleaseHero(Hero hero)
    {
        hero.gameObject.SetActive(false);
    }

    private void OnDestroyHero(Hero hero)
    {
        Destroy(hero.gameObject);
    }
    
    [System.Serializable]
    public class HeroDataList
    {
        public List<HeroData> dataList = new();
    }

    [HideInInspector]
    public List<HeroDataList> heroDataRarityLists = new();
    
    public List<HeroSummonProbabilityData> heroSummonProbabilityDataLists = new();

    private void OnValidate()
    {
        int targetCount = (int)HeroRarity.Count;

        while (heroDataRarityLists.Count < targetCount)
        {
            heroDataRarityLists.Add(new HeroDataList());
        }

        while (heroDataRarityLists.Count > targetCount)
        {
            heroDataRarityLists.RemoveAt(heroDataRarityLists.Count - 1);
        }
    }
}