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

    private IObjectPool<Hero> HeroPool { get; set; }

    private int HeroSummonProbabilityIndex { get; set; }

    private readonly List<Vector3Int> heroSpawnPositionList = new();
    private List<HeroSpawnPointInCell> HeroSpawnPointInCellList { get; } = new();

    private RectTransform heroSummonButtonRectTr;

    public Dictionary<Collider2D, HeroSpawnPointInCell> CurrCellsDict { get; } = new();

    private int currHeroCount;
    public int MaxHeroCount { get; private set; } = 20;

    private void Awake()
    {
        HeroPool = new ObjectPool<Hero>(OnCreateHero, OnGetHero, OnReleaseHero, OnDestroyHero);

        HeroSummonProbabilityIndex = 0;

        foreach (var pair in CurrCellsDict)
            Destroy(pair.Value.gameObject);

        CurrCellsDict.Clear();

        currHeroCount = 0;
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
    }

    public void OnClickCreateHero()
    {
        if (currHeroCount >= MaxHeroCount)
        {
            Debug.Log("Hero count is full");

            return;
        }

        Hero hero = HeroPool.Get();

        bool success = SetHeroDataByRandData(hero);
        if (!success)
        {
            Debug.Assert(false, "SetHeroData Failed");

            return;
        }

        foreach (var cell in HeroSpawnPointInCellList)
        {
            if (cell.CanSpawnHero(hero))
            {
                hero.Initialize();

                inGameUIManager.SetHeroCountText(++currHeroCount, MaxHeroCount);

                return;
            }
        }

        Debug.Log("Cant Spawn Hero");
        HeroPool.Release(hero);
    }

    private bool SetHeroDataByRandData(Hero hero)
    {
        if (HeroSummonProbabilityIndex < 0 || HeroSummonProbabilityIndex >= heroSummonProbabilityDataLists.Count)
            return false;

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

        return false;
    }

    private bool SetHeroData(Hero hero, int listIndex)
    {
        if (listIndex < 0 || listIndex >= heroDataRarityLists.Count)
            return false;

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
}