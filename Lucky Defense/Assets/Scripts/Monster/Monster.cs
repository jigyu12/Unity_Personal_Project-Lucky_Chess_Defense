using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
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
        
        OnDestroy.RemoveAllListeners();
        if (monsterData.MonType == MonsterType.Boss)
            OnDestroy.AddListener(() => waveManager.ReduceBossMonsterCount());
        OnDestroy.AddListener(DestroyMonster);
        
        if (monsterData.MonType == MonsterType.Boss)
            GetComponent<SpriteRenderer>().color = Color.red;
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

        if (currentHp <= 0)
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
}