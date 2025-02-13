using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class Monster : MonoBehaviour
{
    [SerializeField] private MonsterData monsterData;

    [SerializeField] private List<Vector3> waypoint;
    [SerializeField] private int currentWaypointIndex;
    
    private WaveManager waveManager;
    private InGameUIManager inGameUIManager;
    
    private IObjectPool<Monster> monsterPool;

    private int maxHp;
    private int currentHp;
    
    private Camera mainCamera;
    
    public bool IsDead { get; private set; }
    private readonly UnityEvent OnDead = new();
    private readonly UnityEvent OnDestroy = new();
    
    private readonly WaitForFixedUpdate waitForFixedUpdate = new();
    private float destroyTimeAccum;
    private const float DestroyTimeDelay = 0.5f;
    
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Image hpFill;
    private RectTransform hpSliderRectTr;
    [SerializeField] private Vector3 sliderPosOffset;

    private GameObject parent;
    
    public MonsterType Type => monsterData.MonType;

    public Collider2D Coll2D { get; private set; }

    private InGameResourceManager inGameResourceManager;
    
    private MonsterSpumSpawner monsterSpumSpawner;
    private GameObject spumMonsterGo;
    private SPUM_Prefabs spumPrefabs;

    private SortingGroup sortingGroup;
    
    private const int DefaultMonsterSortingOrderOffset = 5;
    
    private readonly Vector3 originalScale = new(0.5f, 0.5f, 1f);
    private readonly Vector3 spumOriginalScale = new(2f, 2f, 1f);
    
    private float originalSpeed;
    private float newSpeed;
    private float currentSlowValue;
    private float currentSlowRate;
    
    private Coroutine slowValueCoroutine;
    private Coroutine slowRateCoroutine;
    
    private bool isStuned;

    private void Awake()
    {
        TryGetComponent(out Collider2D coll);
        Coll2D = coll;
    }
    
    private void OnEnable()
    {
        GameObject.FindGameObjectWithTag("WaveManager").TryGetComponent(out waveManager);
        GameObject.FindGameObjectWithTag("InGameUIManager").TryGetComponent(out inGameUIManager);
        GameObject.FindGameObjectWithTag("InGameResourceManager").TryGetComponent(out inGameResourceManager);
        
        mainCamera = Camera.main;
        
        hpSlider.TryGetComponent(out hpSliderRectTr);
        
        currentWaypointIndex = 0;

        IsDead = false;

        Coll2D.enabled = true;
        
        destroyTimeAccum = 0f;
    }

    private void OnDisable()
    {
        if (spumMonsterGo is not null)
        {
            monsterSpumSpawner?.monsterSpumPoolDict[monsterData.MonsterID].Release(spumMonsterGo);
            spumMonsterGo = null;
        }
    }

    private void Start()
    {
        parent = GameObject.FindGameObjectWithTag("Monsters");
        transform.SetParent(parent.transform);
    }

    private void Update()
    {
        if(IsDead)
            return;
        
        transform.position = Vector3.MoveTowards(transform.position, waypoint[currentWaypointIndex], monsterData.MonSpeed * Time.deltaTime);
        
        hpSliderRectTr.position = mainCamera.WorldToScreenPoint(transform.position + sliderPosOffset * transform.localScale.y);
        
        if (Vector3.Distance(transform.position, waypoint[currentWaypointIndex]) < 0.01f)
        {
            transform.position = waypoint[currentWaypointIndex];
            
            currentWaypointIndex = ++currentWaypointIndex % waypoint.Count;

            if (!Mathf.Approximately(waypoint[currentWaypointIndex].y, transform.position.y))
            {
                var localScale = transform.localScale;
                
                localScale.x = -localScale.x;
                
                transform.localScale = localScale;
            }
        }
    }
    
    /// <summary>
    /// Call this method after the SetMonsterData function is invoked.
    /// </summary>
    public void Initialize()
    {
        maxHp = (int)(monsterData.MonHp * waveManager.CurrHpMultiplier);
        currentHp = maxHp;
        
        hpSlider.maxValue = 1f;
        hpSlider.value = 1f;
        hpFill.color = Color.green;
        
        hpSliderRectTr.position = mainCamera.WorldToScreenPoint(transform.position + sliderPosOffset * transform.localScale.y);
        
        OnDead.RemoveAllListeners();
        OnDead.AddListener(() => { IsDead = true; });
        OnDead.AddListener(() => waveManager.CurrMonstersDict.Remove(Coll2D));
        OnDead.AddListener(() => Coll2D.enabled = false);
        if (monsterData.MonType == MonsterType.Normal)
            OnDead.AddListener(() => inGameUIManager.SetMonsterCountSliderAndText
                (--waveManager.CurrentMonsterCountToSlider, waveManager.MaxMonsterCount));
        if ((InGameResourceType)monsterData.MonReward == InGameResourceType.Coin)
        {
            OnDead.AddListener(() => inGameResourceManager.AddCoin(monsterData.MonReward));
        }
        else if ((InGameResourceType)monsterData.MonReward == InGameResourceType.Gem)
        {
            OnDead.AddListener(() => inGameResourceManager.AddGem(monsterData.MonReward));
        }
        else
            Debug.Assert(false, "Invalid monster reward type");
        OnDead.AddListener(() => spumPrefabs.PlayAnimation(PlayerState.DEATH, 0));
        
        OnDestroy.RemoveAllListeners();
        if (monsterData.MonType == MonsterType.Boss)
            OnDestroy.AddListener(() => waveManager.ReduceBossMonsterCount());
        OnDestroy.AddListener(DestroyMonster);
        
        if(monsterSpumSpawner is null)
            GameObject.FindGameObjectWithTag("MonsterSpumSpawner").TryGetComponent(out monsterSpumSpawner); 
        
        spumMonsterGo = monsterSpumSpawner.monsterSpumPoolDict[monsterData.MonsterID].Get();
        spumMonsterGo.transform.SetParent(transform);
        spumMonsterGo.transform.localPosition = Vector3.zero;
        
        spumMonsterGo.TryGetComponent(out spumPrefabs);
        spumPrefabs.OverrideControllerInit();

        spumMonsterGo.transform.GetChild(0).gameObject.TryGetComponent(out sortingGroup);

        spumPrefabs.PlayAnimation(PlayerState.MOVE, 0);

        if (monsterData.MonType == MonsterType.Normal)
        {
            sortingGroup.sortingOrder = DefaultMonsterSortingOrderOffset;
            
            transform.localScale = originalScale;
            spumMonsterGo.transform.localScale = spumOriginalScale;
        }
        else if (monsterData.MonType == MonsterType.Boss)
        {
            sortingGroup.sortingOrder = DefaultMonsterSortingOrderOffset + 1;
            
            transform.localScale = originalScale * 1.5f;
            spumMonsterGo.transform.localScale = spumOriginalScale * 1.5f;
        }

        bool isFlip = (waypoint[currentWaypointIndex] - transform.position).x > 0f;
        {
            var localScale = transform.localScale;
            localScale.x = isFlip ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
            transform.localScale = localScale;
            
        }
        {
            var localScale = spumMonsterGo.transform.localScale;
            localScale.x = isFlip ? -Mathf.Abs(localScale.x) : Mathf.Abs(localScale.x);
            spumMonsterGo.transform.localScale = localScale;
        }
        
        originalSpeed = monsterData.MonSpeed;
        newSpeed = originalSpeed;
        currentSlowValue = 0f;
        currentSlowRate = 0f;
        
        slowValueCoroutine = null;
        slowRateCoroutine = null;
        isStuned = false;
        
        spumPrefabs._anim.speed = 1f;
    }

    public void SetPool(IObjectPool<Monster> pool)
    {
        monsterPool = pool;
    }

    public void DestroyMonster()
    {
        monsterPool.Release(this);
    }
    
    public void SetWaypoint(List<Vector3> newWayPoint)
    {
        waypoint = newWayPoint;
    }

    public void OnDamaged(int damage)
    {
        currentHp -= damage;
        
        hpSlider.value = (float)currentHp / maxHp;
        hpFill.color = Color.Lerp(Color.red,Color.green , hpSlider.value);

        if (!IsDead && currentHp <= 0)
            StartCoroutine(OnDeadCoroutine());
    }

    private IEnumerator OnDeadCoroutine()
    {
        OnDead?.Invoke();
        
        while (destroyTimeAccum <  DestroyTimeDelay)
        {
            yield return waitForFixedUpdate;

            destroyTimeAccum += Time.deltaTime;
        }

        OnDestroy?.Invoke();
    }

    public void SetMonsterData(MonsterData newMonsterData)
    {
        monsterData = newMonsterData;
    }

    public void ReduceMoveSpeedValue(float value, float durationTime, float probability)
    {
        if (Random.value > probability * 0.01f)
            return;
    
        if (slowValueCoroutine != null)
        {
            if (Mathf.Approximately(currentSlowValue, value))
            {
                StopCoroutine(slowValueCoroutine);
                slowValueCoroutine = StartCoroutine(CalculateSlowValue(durationTime, value));
                
                return;
            }
            if (value < currentSlowValue)
            {
                return;
            }
            StopCoroutine(slowValueCoroutine);
        }
    
        currentSlowValue = value;
        UpdateMoveSpeed();
        slowValueCoroutine = StartCoroutine(CalculateSlowValue(durationTime, value));
    }
    
    private IEnumerator CalculateSlowValue(float durationTime, float value)
    {
        yield return new WaitForSeconds(durationTime);
    
        if (Mathf.Approximately(currentSlowValue, value))
        {
            currentSlowValue = 0f;
            UpdateMoveSpeed();
        
            spumPrefabs.PlayAnimation(PlayerState.MOVE, 0);
            
            isStuned = false;
            slowValueCoroutine = null;
        }
    }
    
    public void ReduceMoveSpeedRate(float value, float durationTime, float probability)
    {
        if (Random.value > probability * 0.01f)
            return;
    
        if (slowRateCoroutine != null)
        {
            if (Mathf.Approximately(currentSlowRate, value))
            {
                StopCoroutine(slowRateCoroutine);
                slowRateCoroutine = StartCoroutine(CalculateSlowRate(durationTime, value));
                return;
            }
            if (value < currentSlowRate)
            {
                return;
            }
            StopCoroutine(slowRateCoroutine);
        }
    
        currentSlowRate = value;
        UpdateMoveSpeed();
        slowRateCoroutine = StartCoroutine(CalculateSlowRate(durationTime, value));
    }
    
    private IEnumerator CalculateSlowRate(float durationTime, float value)
    {
        yield return new WaitForSeconds(durationTime);
    
        if (Mathf.Approximately(currentSlowRate, value))
        {
            currentSlowRate = 0f;
            UpdateMoveSpeed();
            
            spumPrefabs.PlayAnimation(PlayerState.MOVE, 0);
            
            isStuned = false;
            slowRateCoroutine = null;
        }
    }
    
    private void UpdateMoveSpeed()
    {
        newSpeed = (originalSpeed - currentSlowValue) - (originalSpeed * currentSlowRate);
        newSpeed = Mathf.Max(0f, newSpeed);
        
        float moveAnimSpeed = newSpeed / originalSpeed;
        spumPrefabs._anim.speed = moveAnimSpeed;

        if (Mathf.Approximately(newSpeed, 0f))
        {
            if (!isStuned)
            {
                spumPrefabs.PlayAnimation(PlayerState.DEBUFF, 0);
                isStuned = true;
            }
        }
        else
        {
            isStuned = false;
        }
    }
}