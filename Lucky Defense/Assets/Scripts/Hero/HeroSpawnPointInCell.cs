using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HeroSpawnPointInCell : MonoBehaviour
{
    [SerializeField] private GameObject singlePos;

    [SerializeField] private GameObject doublePos0;
    [SerializeField] private GameObject doublePos1;

    [SerializeField] private GameObject triplePos0;
    [SerializeField] private GameObject triplePos1;
    [SerializeField] private GameObject triplePos2;

    private readonly List<GameObject> singlePosList = new();
    private readonly List<GameObject> doublePosList = new();
    private readonly List<GameObject> triplePosList = new();

    public int Cost { get; private set; }
    public int HeroCount { get; private set; }
    public int OccupyHeroId { get; private set; } = -1;
    public float AttackRange { get; private set; } = 0f;

    public static readonly int DefaultOccupyHeroId = -1;
    private const float DefaultAttackRange = 0f;

    [SerializeField] private List<Hero> heroList;

    private const int HeroCountMax = 3;

    [SerializeField] private SpriteRenderer circleRenderer;
    [SerializeField] private SpriteRenderer circleOutlineRenderer;
    [SerializeField] private float outlineThickness = 0.025f;
    private float circleSpeed;
    private bool isCellSwapping;

    private Camera mainCamera;

    private GameObject parent;

    [SerializeField] private SpriteRenderer squareRenderer;
    public Collider2D Coll2D { get; private set; }

    [SerializeField] private LayerMask cellLayer;

    [SerializeField] private Button heroSellButton;
    private RectTransform heroSellButtonRectTr;
    [SerializeField] private Vector3 heroSellButtonPosOffset;

    [SerializeField] private Button heroFusionButton;
    private RectTransform heroFusionButtonRectTr;
    [SerializeField] private Vector3 heroFusionButtonPosOffset;

    private InGameResourceManager inGameResourceManager;
    private HeroSpawner heroSpawner;

    private delegate void HeroSellEvent(HeroSpawnPointInCell sellerCell);

    private static event HeroSellEvent OnHeroSellEvent;
    [SerializeField] private TMP_Text sellTypeInHeroSellButtonText;
    private StringBuilder stringBuilder;
    [SerializeField] private Image sellTypeInHeroSellButtonImage;
    private InGameUIManager inGameUIManager;

    public static UnityEvent<HeroSpawnPointInCell, bool> OnOccupyHeroIdChangedInCell = new();

    private SkillManager skillManager;

    public int currCellSynergyClass;
    
#if UNITY_STANDALONE || UNITY_EDITOR

    [SerializeField] private TMP_Text occupyHeroIdDebugText;
    private RectTransform occupyHeroIdDebugTextRectTr;

#endif

    private void Awake()
    {
        heroList.Capacity = HeroCountMax;

        TryGetComponent(out Collider2D coll);
        Coll2D = coll;

        isCellSwapping = false;

        singlePosList.Clear();
        doublePosList.Clear();
        triplePosList.Clear();

        heroSellButton.TryGetComponent(out heroSellButtonRectTr);
        heroSellButton.onClick.RemoveAllListeners();
        heroSellButton.onClick.AddListener(OnClickSellHero);

        heroFusionButton.TryGetComponent(out heroFusionButtonRectTr);
        heroFusionButton.onClick.RemoveAllListeners();
        heroFusionButton.onClick.AddListener(OnClickFusionHero);

        stringBuilder = new();
        stringBuilder.Clear();
        
        currCellSynergyClass = -1;

#if UNITY_STANDALONE || UNITY_EDITOR

        occupyHeroIdDebugText.TryGetComponent(out occupyHeroIdDebugTextRectTr);

#endif
    }

    private void OnEnable()
    {
        ClearCell();

        OnHeroSellEvent += HandleHeroSellEvent;
    }

    private void OnDisable()
    {
        OnHeroSellEvent -= HandleHeroSellEvent;
    }

    private void ClearCell()
    {
        OnOccupyHeroIdChangedInCell?.Invoke(this, false);
        currCellSynergyClass = -1;
        
        foreach (Hero hero in heroList)
            hero.DestroyHero();

        heroList.Clear();

        Cost = 3;
        HeroCount = 0;

        if (heroSpawner is not null && heroSpawner.CellsByOccupyHeroIdDict.ContainsKey(OccupyHeroId))
            heroSpawner.CellsByOccupyHeroIdDict[OccupyHeroId].Remove(this);
        OccupyHeroId = DefaultOccupyHeroId;
        AttackRange = DefaultAttackRange;

        HideAttackRangeCircle();
        HideHighlightMoveCell();
        HideHeroSellButton();
        HideHeroFusionButton();
        if(inGameUIManager is not null)
            HideCurrHeroInfoPanel();

#if UNITY_STANDALONE || UNITY_EDITOR

        occupyHeroIdDebugText.SetText(OccupyHeroId.ToString());

#endif
    }

    private void Start()
    {
        parent = GameObject.FindGameObjectWithTag("Cells");
        transform.SetParent(parent.transform);

        mainCamera = Camera.main;

        singlePosList.Add(singlePos);

        doublePosList.Add(doublePos0);
        doublePosList.Add(doublePos1);

        triplePosList.Add(triplePos0);
        triplePosList.Add(triplePos1);
        triplePosList.Add(triplePos2);

        circleSpeed = Hero.HeroSwapSpeed;

        GameObject.FindGameObjectWithTag("InGameResourceManager").TryGetComponent(out inGameResourceManager);
        GameObject.FindGameObjectWithTag("HeroSpawner").TryGetComponent(out heroSpawner);
        GameObject.FindGameObjectWithTag("InGameUIManager").TryGetComponent(out inGameUIManager);
        HideCurrHeroInfoPanel();

        SynergyManager.OnSynergyCalcualted.AddListener(OnSynergyCalcualtedHandler);
        GameObject.FindGameObjectWithTag("SkillManager").TryGetComponent(out skillManager);
    }

    private void Update()
    {
        heroSellButtonRectTr.position =
            mainCamera.WorldToScreenPoint(singlePos.transform.position +
                                          heroSellButtonPosOffset * transform.localScale.y);
        heroFusionButtonRectTr.position =
            mainCamera.WorldToScreenPoint(singlePos.transform.position +
                                          heroFusionButtonPosOffset * transform.localScale.y);

        DetectTouch();

        CircleMoveToCurrCellPos();

#if UNITY_STANDALONE || UNITY_EDITOR

        occupyHeroIdDebugTextRectTr.position =
            mainCamera.WorldToScreenPoint(singlePos.transform.position * transform.localScale.y);

        if (DebugModeUI.IsDebugMode)
        {
            occupyHeroIdDebugText.gameObject.SetActive(true);
        }
        else
        {
            occupyHeroIdDebugText.gameObject.SetActive(false);
        }

#endif
    }

    private void DetectTouch()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            Vector2 touchWorldPosition = mainCamera.ScreenToWorldPoint(touch.position);

            Collider2D hitCollider = Physics2D.OverlapPoint(touchWorldPosition, cellLayer);
            if (touch.phase == TouchPhase.Began)
            {
                if (hitCollider?.gameObject == gameObject && !Mathf.Approximately(AttackRange, DefaultAttackRange))
                {
                    if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                        return;
                    
                    ShowAttackRangeCircle();
                    ShowHeroSellButton();
                    ShowCurrHeroInfoPanel();

                    if (heroList[0].HeroGrade != HeroGrade.Mythic)
                        ShowHeroFusionButton();
                }
                else
                {
                    HideCurrHeroInfoPanel();
                    
                    if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                        return;

                    HideAttackRangeCircle();
                    HideHeroSellButton();
                    HideHeroFusionButton();
                }
            }
        }
    }

    public void ShowHighlightMoveCell()
    {
        squareRenderer.gameObject.SetActive(true);
    }

    public void HideHighlightMoveCell()
    {
        squareRenderer.gameObject.SetActive(false);
    }

    private void ShowAttackRangeCircle()
    {
        float spriteSize = circleRenderer.sprite.bounds.size.x;
        circleRenderer.gameObject.transform.localScale =
            Vector3.one * ((AttackRange - outlineThickness) * 2 / spriteSize);

        float outlineSpriteSize = AttackRange * 2 / spriteSize;
        circleOutlineRenderer.gameObject.transform.localScale = Vector3.one * outlineSpriteSize;

        circleRenderer.gameObject.SetActive(true);
        circleOutlineRenderer.gameObject.SetActive(true);
    }

    private void HideAttackRangeCircle()
    {
        circleRenderer.gameObject.SetActive(false);
        circleOutlineRenderer.gameObject.SetActive(false);
    }

    private void ShowHeroSellButton()
    {
        if (isCellSwapping)
            return;

        heroSellButton.gameObject.SetActive(true);
    }

    private void HideHeroSellButton()
    {
        heroSellButton.gameObject.SetActive(false);
    }

    private void ShowHeroFusionButton()
    {
        if (isCellSwapping)
            return;

        heroFusionButton.gameObject.SetActive(true);
    }

    private void HideHeroFusionButton()
    {
        heroFusionButton.gameObject.SetActive(false);
    }

    public void MoveCell(Vector3 destPos)
    {
        HideHeroSellButton();
        HideHeroFusionButton();

        circleRenderer.transform.SetParent(null);
        circleOutlineRenderer.transform.SetParent(null);

        isCellSwapping = true;

        bool? heroAnimFlipVal;
        if (Mathf.Approximately(transform.position.x, destPos.x))
            heroAnimFlipVal = null;
        else
            heroAnimFlipVal = transform.position.x > destPos.x;

        transform.position = destPos;

        PlaceHero(false, heroAnimFlipVal);
    }

    private void CircleMoveToCurrCellPos()
    {
        if (!isCellSwapping)
            return;

        Vector3 destPosition = singlePos.transform.position;

        circleRenderer.transform.position = Vector3.MoveTowards(circleRenderer.transform.position, destPosition,
            circleSpeed * Time.deltaTime);
        circleOutlineRenderer.transform.position = Vector3.MoveTowards(circleOutlineRenderer.transform.position,
            destPosition,
            circleSpeed * Time.deltaTime);

        if (Vector3.Distance(circleRenderer.transform.position, destPosition) < 0.01f)
        {
            isCellSwapping = false;

            circleRenderer.transform.position = destPosition;
            circleOutlineRenderer.transform.position = destPosition;

            circleRenderer.transform.SetParent(transform);
            circleOutlineRenderer.transform.SetParent(transform);
        }
    }

    public bool CanSpawnHero(Hero hero)
    {
        if (OccupyHeroId == DefaultOccupyHeroId)
            return CalculatePlaceHeroByCostAndCount(hero);

        return OccupyHeroId == hero.HeroId && CalculatePlaceHeroByCostAndCount(hero);
    }

    private bool CalculatePlaceHeroByCostAndCount(Hero hero)
    {
        if (Cost < hero.Cost || HeroCount >= HeroCountMax)
            return false;

        AddHero(hero);

        return true;
    }

    private void PlaceHero(bool teleport = true, bool? heroAnimFlipVal = null)
    {
        switch (HeroCount)
        {
            case 0:
                break;
            case 1:
            {
                for (int i = 0; i < singlePosList.Count; ++i)
                {
                    if (teleport)
                    {
                        if (heroList[i].IsMoving)
                            heroList[i].destPosition = singlePosList[i].transform.position;
                        else
                            heroList[i].transform.position = singlePosList[i].transform.position;
                    }
                    else
                    {
                        heroList[i].destPosition = singlePosList[i].transform.position;

                        heroList[i].IsMoving = true;
                        heroList[i].SetHeroAnimMove(heroAnimFlipVal);
                    }
                }
            }
                break;
            case 2:
            {
                for (int i = 0; i < doublePosList.Count; ++i)
                {
                    if (teleport)
                    {
                        if (heroList[i].IsMoving)
                            heroList[i].destPosition = doublePosList[i].transform.position;
                        else
                            heroList[i].transform.position = doublePosList[i].transform.position;
                    }
                    else
                    {
                        heroList[i].destPosition = doublePosList[i].transform.position;

                        heroList[i].IsMoving = true;
                        heroList[i].SetHeroAnimMove(heroAnimFlipVal);
                    }
                }
            }
                break;
            case 3:
            {
                for (int i = 0; i < triplePosList.Count; ++i)
                {
                    if (teleport)
                    {
                        if (heroList[i].IsMoving)
                            heroList[i].destPosition = triplePosList[i].transform.position;
                        else
                            heroList[i].transform.position = triplePosList[i].transform.position;
                    }
                    else
                    {
                        heroList[i].destPosition = triplePosList[i].transform.position;

                        heroList[i].IsMoving = true;
                        heroList[i].SetHeroAnimMove(heroAnimFlipVal);
                    }
                }
            }
                break;
            default:
                Debug.Assert(false, "Invalid Hero Count");
                break;
        }
    }

    public Vector3 GetCenterPos()
    {
        return singlePos.transform.position;
    }

    private void OnClickSellHero()
    {
        if (heroList.Count <= 0)
        {
            Debug.Assert(false, "The hero list is empty, but try selling a hero.");

            return;
        }

        Hero removedHero = RemoveLastHero();
        removedHero.DestroyHero();

        OnHeroSellEvent?.Invoke(this);
    }

    private void HandleHeroSellEvent(HeroSpawnPointInCell sellerCell)
    {
        if (this == sellerCell)
            return;

        if (OccupyHeroId != sellerCell.OccupyHeroId)
            return;

        if (0 < HeroCount && HeroCount < HeroCountMax)
        {
            sellerCell.AddHero(RemoveLastHero(true));
        }
    }

    private void AddHero(Hero hero, bool teleport = true)
    {
        bool isOccupyHeroIdChanged = false;
        
        if (OccupyHeroId == DefaultOccupyHeroId)
        {
            isOccupyHeroIdChanged = true;
            
            OccupyHeroId = hero.HeroId;

            stringBuilder.Clear();
            stringBuilder.Append(hero.SaleQuantity);
            sellTypeInHeroSellButtonText.SetText(stringBuilder.ToString());

            if (!heroSpawner.CellsByOccupyHeroIdDict.ContainsKey(OccupyHeroId))
            {
                List<HeroSpawnPointInCell> newList = new();
                newList.Add(this);
                heroSpawner.CellsByOccupyHeroIdDict.Add(OccupyHeroId, newList);
            }
            else if (!heroSpawner.CellsByOccupyHeroIdDict[OccupyHeroId].Contains(this))
            {
                heroSpawner.CellsByOccupyHeroIdDict[OccupyHeroId].Add(this);
            }

            sellTypeInHeroSellButtonImage.sprite = inGameUIManager.inGameResourceIcons[hero.SaleType - 1];
        }

        Cost -= hero.Cost;
        AttackRange = hero.AttackRange;
        ++HeroCount;
        if (HeroCount == HeroCountMax)
            heroFusionButton.interactable = true;

        heroList.Add(hero);

        if (isOccupyHeroIdChanged)
        {
            currCellSynergyClass = heroList[0].SynergyClass1;
                
            OnOccupyHeroIdChangedInCell?.Invoke(this, true);
        }

        PlaceHero(teleport);

#if UNITY_STANDALONE || UNITY_EDITOR

        occupyHeroIdDebugText.SetText(OccupyHeroId.ToString());

#endif
    }

    private Hero RemoveLastHero(bool isRemovedHeroMoveToOtherCell = false, bool teleport = true)
    {
        var lastHero = heroList[heroList.Count - 1];

        if (heroList.Count == 1)
        {
            heroSpawner.CellsByOccupyHeroIdDict[OccupyHeroId].Remove(this);
            
            OnOccupyHeroIdChangedInCell?.Invoke(this, false);
            currCellSynergyClass = -1;

            OccupyHeroId = DefaultOccupyHeroId;
            AttackRange = DefaultAttackRange;

            HideAttackRangeCircle();
            HideHeroSellButton();
            HideHeroFusionButton();
            HideCurrHeroInfoPanel();

            stringBuilder.Clear();
            stringBuilder.Append(00);
            sellTypeInHeroSellButtonText.SetText(stringBuilder.ToString());
        }

        Cost += lastHero.Cost;
        --HeroCount;
        if (!isRemovedHeroMoveToOtherCell)
        {
            heroSpawner.RemoveCurrHeroCount(1);

            if ((InGameResourceType)lastHero.SaleType == InGameResourceType.Coin)
                inGameResourceManager.AddCoin(lastHero.SaleQuantity);
            else if ((InGameResourceType)lastHero.SaleType == InGameResourceType.Gem)
                inGameResourceManager.AddGem(lastHero.SaleQuantity);
            else
                Debug.Assert(false, "Invalid Sale Quantity In Last Hero.");
        }

        heroFusionButton.interactable = false;

        heroList.RemoveAt(heroList.Count - 1);

        PlaceHero(teleport);

#if UNITY_STANDALONE || UNITY_EDITOR

        occupyHeroIdDebugText.SetText(OccupyHeroId.ToString());

#endif

        return lastHero;
    }

    private void OnClickFusionHero()
    {
        HeroGrade nextGrade = heroList[0].HeroGrade + 1;
        if (nextGrade == HeroGrade.Mythic)
            HideHeroFusionButton();
        else
            heroFusionButton.interactable = false;

        heroSpawner.RemoveCurrHeroCount(HeroCountMax);
        var newHero = heroSpawner.OnClickCreateHero(true, 100f, nextGrade, false, false);

        ClearCell();

        heroSpawner.SpawnHeroInit(newHero);

        if (heroSpawner.CellsByOccupyHeroIdDict.ContainsKey(newHero.HeroId))
        {
            foreach (var cell in heroSpawner.CellsByOccupyHeroIdDict[newHero.HeroId])
            {
                bool canSpawnHero = cell.CanSpawnHero(newHero);
                if (canSpawnHero)
                {
                    cell.ShowAttackRangeCircle();

                    heroSpawner.SortHeroesInCellDrawOrder();

                    return;
                }
            }
        }

        AddHero(newHero);
        
        heroSpawner.SortHeroesInCellDrawOrder();

        ShowAttackRangeCircle();
        ShowCurrHeroInfoPanel();
    }

    public void SortHeroesDrawOrder(int drawOrder)
    {
        foreach (var hero in heroList)
        {
            hero.SetHeroDrawOrder(drawOrder++);
        }
    }
    
    /// <param name="isAddSynergy">If this value is true, add the synergy; if false, remove the synergy.</param>
    private void OnSynergyCalcualtedHandler(List<int> skillIdList, bool isAddSynergy, int cellSynergyClass1)
    {
        if (OccupyHeroId == DefaultOccupyHeroId)
            return;

        foreach (var skillId in skillIdList)
        {
            var passiveSkillData = skillManager.AllPassiveSkillDict[skillId];
            
            if (((SkillTargetType)passiveSkillData.TargetType == SkillTargetType.AllAlly)
                || ((SkillTargetType)passiveSkillData.TargetType == SkillTargetType.SpecificAlly &&
                    currCellSynergyClass == cellSynergyClass1))
            {
                if ((SkillEffectType)passiveSkillData.EffectType is
                    SkillEffectType.AtkValue or SkillEffectType.AtkSpeedValue)
                {
                    var statValueData = skillManager.StatValuePassiveSkillDict[skillId];

                    if (isAddSynergy)
                    {
                        foreach (var hero in heroList)
                        {
                            hero.SetValueDataList(statValueData, true);
                        }
                    }
                    else
                    {
                        foreach (var hero in heroList)
                        {
                            hero.SetValueDataList(statValueData, false);
                        }
                    }
                }
                else if ((SkillEffectType)passiveSkillData.EffectType is
                         SkillEffectType.AtkRate or SkillEffectType.AtkSpeedRate)
                {
                    var statRateData = skillManager.StatRatePassiveSkillDict[skillId];

                    if (isAddSynergy)
                    {
                        foreach (var hero in heroList)
                        {
                            hero.SetRateDataList(statRateData, true);
                        }
                    }
                    else
                    {
                        foreach (var hero in heroList)
                        {
                            hero.SetRateDataList(statRateData, false);
                        }
                    }
                }
                else if ((SkillEffectType)passiveSkillData.EffectType is
                         SkillEffectType.AcquireCoin or SkillEffectType.AcquireGem)
                {
                    var additionalAttack = skillManager.AttackPassiveSkillDict[skillId];

                    if (isAddSynergy)
                    {
                        foreach (var hero in heroList)
                        {
                            hero.SetOnAdditionalAttack(additionalAttack, true);
                        }
                    }
                    else
                    {
                        foreach (var hero in heroList)
                        {
                            hero.SetOnAdditionalAttack(additionalAttack, false);
                        }
                    }
                }
            }
            else if ((SkillTargetType)passiveSkillData.TargetType == SkillTargetType.AllEnemy)
            {
                if ((SkillEffectType)passiveSkillData.EffectType is
                         SkillEffectType.SpeedValue or SkillEffectType.SpeedRate)
                {
                    var attackSkillToMon = skillManager.AttackSkillToMonDict[skillId];

                    if (isAddSynergy)
                    {
                        foreach (var hero in heroList)
                        {
                            hero.SetOnAttackToMon(attackSkillToMon, true);
                        }
                    }
                    else
                    {
                        foreach (var hero in heroList)
                        {
                            hero.SetOnAttackToMon(attackSkillToMon, false);
                        }
                    }
                }
            }
        }
    }
    
    private void ShowCurrHeroInfoPanel()
    {
        inGameUIManager.SetHeroInfoPanelActive(this, heroList[0].GetCurrHeroInfoData());
    }

    private void HideCurrHeroInfoPanel()
    {
        inGameUIManager.SetHeroInfoPanelInactive(this);
    }
}