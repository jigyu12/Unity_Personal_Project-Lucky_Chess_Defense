using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class HeroSpawner : MonoBehaviour
{
    [SerializeField] private Button heroSummonButton;

    [SerializeField] private GameObject heroPrefab;
    [SerializeField] private GameObject heroSpawnPointInCellPrefab;

    [SerializeField] private Tilemap heroSpawnTilemap;

    [SerializeField] private InGameUIManager inGameUIManager;

    [SerializeField] private InGameResourceManager inGameResourceManager;
    
    private IObjectPool<Hero> HeroPool { get; set; }

    private int HeroSummonProbabilityIndex { get; set; }

    private readonly List<Vector3Int> heroSpawnPositionList = new();
    private List<HeroSpawnPointInCell> HeroSpawnPointInCellList { get; } = new();

    private RectTransform heroSummonButtonRectTr;

    public Dictionary<Collider2D, HeroSpawnPointInCell> CurrCellsDict { get; } = new();

    private int currHeroCount;
    public int MaxHeroCount { get; private set; } = 20;

    public HeroSummonProbabilityData CurrentHeroSummonProbabilityData
    {
        get;
        private set;
    }

    public int CurrProbabilityLevel => HeroSummonProbabilityIndex + 1;
    public int MaxProbabilityLevel => heroSummonProbabilityDataLists.Count;
    
    [SerializeField] private Button probabilityEnforceButton;

    public float RareSummonOnlyProbability => 60f;
    public float HeroicSummonOnlyProbability => 20f;
    public float LegendarySummonOnlyProbability => 10f;

    public Dictionary<int, List<HeroSpawnPointInCell>> CellsByOccupyHeroIdDict { get; } = new();

    private void Awake()
    {
        HeroPool = new ObjectPool<Hero>(OnCreateHero, OnGetHero, OnReleaseHero, OnDestroyHero);

        HeroSummonProbabilityIndex = 0;

        foreach (var pair in CurrCellsDict)
            Destroy(pair.Value.gameObject);

        CurrCellsDict.Clear();

        currHeroCount = 0;
        
        CurrentHeroSummonProbabilityData = heroSummonProbabilityDataLists[HeroSummonProbabilityIndex];
        
        CellsByOccupyHeroIdDict.Clear();
    }

    private void Start()
    {
        heroSummonButton.TryGetComponent(out heroSummonButtonRectTr);
        RectTransformUtility.ScreenPointToWorldPointInRectangle(heroSummonButtonRectTr, heroSummonButtonRectTr.position,
            Camera.main, out Vector3 position);
        transform.position = position;

        heroSpawnPositionList.Clear();

        foreach (var cell in HeroSpawnPointInCellList)
            Destroy(cell.gameObject);
        HeroSpawnPointInCellList.Clear();

        BoundsInt bounds = heroSpawnTilemap.cellBounds;
        foreach (Vector3Int cellPos in bounds.allPositionsWithin)
        {
            if (heroSpawnTilemap.HasTile(cellPos))
                heroSpawnPositionList.Add(cellPos);
        }

        heroSpawnPositionList.Sort((a, b) =>
        {
            if (a.y != b.y)
                return b.y.CompareTo(a.y);

            return a.x.CompareTo(b.x);
        });

        foreach (var pos in heroSpawnPositionList)
        {
            Instantiate(heroSpawnPointInCellPrefab).TryGetComponent(out HeroSpawnPointInCell cell);
            HeroSpawnPointInCellList.Add(cell);
            CurrCellsDict.Add(cell.Coll2D, cell);

            cell.transform.position = pos;
        }

        inGameUIManager.SetHeroCountText(currHeroCount, MaxHeroCount);
        inGameUIManager.SetProbabilityTexts();
        inGameUIManager.SetLuckySummonGemCostTexts();
    }
    
    /// <param name="isLuckySummon">If this value is true, do a lucky summon; if it is false(default), do a random summon.</param>
    /// <param name="probability">Assign this value only when isLuckySummon is true.</param>
    /// <param name="heroGrade">Assign this value only when isLuckySummon is true.</param>
    /// /// <param name="useCoin">Assign this value as false only when isLuckySummon is true. If this value is false, use the gem instead.</param>
    public void OnClickCreateHero(bool isLuckySummon = false, float? probability = null, HeroGrade? heroGrade = null, bool useCoin = true)
    {
        if (useCoin)
        {
            bool canUseCoin = inGameResourceManager.TryUseCoin(inGameResourceManager.CurrentHeroSummonCoinCost);
            if (!canUseCoin)
            {
                Debug.Log("Not enough coins to summon a hero.");

                return;
            }
        }
        else
        {
            int gemCost;
            switch (heroGrade)
            {
                case HeroGrade.Rare:
                    gemCost = inGameResourceManager.InitialRareSummonGemCost;
                    break;
                case HeroGrade.Heroic:
                    gemCost = inGameResourceManager.InitialHeroicSummonGemCost;
                    break;
                case HeroGrade.Legendary:
                    gemCost = inGameResourceManager.InitialLegendarySummonGemCost;
                    break;
                default:
                    Debug.Log("Invalid Hero Grade for luckySummon.");
                    return;
            }
            
            bool canUseGem = inGameResourceManager.TryUseGem(gemCost);
            if (!canUseGem)
            {
                Debug.Log("Not enough gems to summon a hero.");

                return;
            }
        }
        
        if (currHeroCount >= MaxHeroCount)
        {
            Debug.Log("Hero count is full");

            return;
        }

        Hero hero = HeroPool.Get();

        bool? success;
        if (isLuckySummon)
            success = SetHeroDataByLuckySummon(hero, probability, heroGrade);
        else
            success = SetHeroDataByRandData(hero);

        if (success is false)
        {
            Debug.Log("Lucky Summon failed.....");
            
            HeroPool.Release(hero);
            
            return;
        }
        
        if (success is null)
        {
            HeroPool.Release(hero);
            
            Debug.Assert(false, "SetHeroData Failed");
            
            return;
        }

        if (CellsByOccupyHeroIdDict.ContainsKey(hero.HeroId))
        {
            foreach (var cell in CellsByOccupyHeroIdDict[hero.HeroId])
            {
                bool canSpawnHeroInCell = CanSpawnHeroInCell(cell, hero);
                if(canSpawnHeroInCell)
                    return;
            }
        }
        
        foreach (var cell in HeroSpawnPointInCellList)
        {
            bool canSpawnHeroInCell = CanSpawnHeroInCell(cell, hero);
            if(canSpawnHeroInCell)
                return;
        }

        Debug.Log("There is no cell available to spawn a hero.");
        HeroPool.Release(hero);
    }

    private bool CanSpawnHeroInCell(HeroSpawnPointInCell cell, Hero hero)
    {
        if (cell.CanSpawnHero(hero))
        {
            hero.Initialize();

            inGameUIManager.SetHeroCountText(++currHeroCount, MaxHeroCount);
                
            inGameResourceManager.AddHeroSummonCoinCost();
                
            Debug.Log($"Summoned hero ID : {hero.HeroId}");

            return true;
        }
        
        return false;
    }

    public void OnClickEnforceProbability()
    {
        bool canUseCoin = inGameResourceManager.TryUseCoin(CurrentHeroSummonProbabilityData.EnforceCost);
        if (!canUseCoin)
        {
            Debug.Log("Not enough coins to enforce a probability.");

            return;
        }

        ++HeroSummonProbabilityIndex;
        
        CurrentHeroSummonProbabilityData = heroSummonProbabilityDataLists[HeroSummonProbabilityIndex];
        
        if (CurrProbabilityLevel == MaxProbabilityLevel)
        {
            probabilityEnforceButton.interactable = false;
        }

        inGameUIManager.SetProbabilityTexts();
    }

    private bool? SetHeroDataByRandData(Hero hero)
    {
        if (HeroSummonProbabilityIndex < 0 || HeroSummonProbabilityIndex >= heroSummonProbabilityDataLists.Count)
            return null;

        HeroSummonProbabilityData pData = heroSummonProbabilityDataLists[HeroSummonProbabilityIndex];

        var properties = typeof(HeroSummonProbabilityData).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        List<float> probabilities = new(properties.Length);
        foreach (var property in properties)
        {
            if (property.PropertyType == typeof(float) && property.Name.EndsWith("Probability"))
            {
                float value = (float)property.GetValue(pData);
                probabilities.Add(value);
            }
        }

        float probability = Random.value * 100f;
        float pSum = 0f;
        for (int i = 0; i < Utility.HeroGradeCount; ++i)
        {
            pSum += probabilities[i];

            if (probability <= pSum)
            {
                return SetHeroData(hero, i);
            }
        }

        return null;
    }

    private bool? SetHeroData(Hero hero, int listIndex)
    {
        if (listIndex < 0 || listIndex >= heroDataRarityLists.Count)
            return null;

        var dataList = heroDataRarityLists[listIndex].dataList;
        hero.SetHeroData(dataList[Random.Range(0, dataList.Count)]);

        return true;
    }

    private Hero OnCreateHero()
    {
        Instantiate(heroPrefab).TryGetComponent(out Hero hero);

        hero.SetPool(HeroPool);

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

    public void SortHeroSpawnPointInCellList()
    {
        HeroSpawnPointInCellList.Sort(CellPositionCmp);
    }

    private int CellPositionCmp(HeroSpawnPointInCell cell1, HeroSpawnPointInCell cell2)
    {
        Vector3 cell1Pos = cell1.transform.position;
        Vector3 cell2Pos = cell2.transform.position;

        if (System.Math.Abs(cell1Pos.y - cell2Pos.y) > 0.0001f)
            return cell2Pos.y.CompareTo(cell1Pos.y);

        return cell1Pos.x.CompareTo(cell2Pos.x);
    }

    [System.Serializable]
    public class HeroDataList
    {
        public List<HeroData> dataList = new();
    }

    [HideInInspector] public List<HeroDataList> heroDataRarityLists = new();

    public List<HeroSummonProbabilityData> heroSummonProbabilityDataLists = new();

    private void OnValidate()
    {
        int targetCount = Utility.HeroGradeCount;

        while (heroDataRarityLists.Count < targetCount)
        {
            heroDataRarityLists.Add(new HeroDataList());
        }

        while (heroDataRarityLists.Count > targetCount)
        {
            heroDataRarityLists.RemoveAt(heroDataRarityLists.Count - 1);
        }
    }

    private bool? SetHeroDataByLuckySummon(Hero hero, float? probability, HeroGrade? heroGrade)
    {
        if (probability is null || heroGrade is null)
            return null;

        float randProbability = Random.value * 100f;

        if (probability < randProbability)
        {
            return false;
        }
        
        return SetHeroData(hero, (int)heroGrade - 1);
    }

    public void RemoveOneCurrHeroCount()
    {
        inGameUIManager.SetHeroCountText(--currHeroCount, MaxHeroCount);
    }
}