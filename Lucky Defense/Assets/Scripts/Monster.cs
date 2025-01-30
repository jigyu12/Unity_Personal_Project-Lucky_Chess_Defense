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
    private const float DestroyTimeDelay = 1f;
    
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Image hpFill;
    private RectTransform hpSliderRectTr;
    private readonly Vector3 sliderPosOffset = new(0, 0.3f, 0);

    private GameObject parent;
    
    public MonsterType Type => monsterData.Type;

    public Collider2D Coll2D { get; private set; }

    private void Awake()
    {
        TryGetComponent(out Collider2D coll);
        Coll2D = coll;
    }
    
    private void OnEnable()
    {
        GameObject.FindGameObjectWithTag("WaveManager").TryGetComponent(out waveManager);
        GameObject.FindGameObjectWithTag("InGameUIManager").TryGetComponent(out inGameUIManager);
        
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
        
        OnDead.RemoveAllListeners();
        OnDead.AddListener(() => { IsDead = true; });
        OnDead.AddListener(() => waveManager.CurrMonstersDict.Remove(Coll2D));
        OnDead.AddListener(() => Coll2D.enabled = false);
        if (monsterData.Type == MonsterType.Normal)
            OnDead.AddListener(() => inGameUIManager.SetMonsterCountSliderAndText
                (--waveManager.CurrentMonsterCountToSlider, waveManager.MaxMonsterCount));
        
        OnDestroy.RemoveAllListeners();
        OnDestroy.AddListener(DestroyMonster);
    }

    private void Update()
    {
        if(IsDead)
            return;
        
        transform.position = Vector3.MoveTowards(transform.position, waypoint[currentWaypointIndex], monsterData.Speed * Time.deltaTime);
        
        hpSliderRectTr.position = mainCamera.WorldToScreenPoint(transform.position + sliderPosOffset);
        
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
        maxHp = (int)(monsterData.Hp * waveManager.CurrHpMultiplier);
        currentHp = maxHp;
        
        hpSlider.maxValue = 1f;
        hpSlider.value = 1f;
        hpFill.color = Color.green;
        
        hpSliderRectTr.position = mainCamera.WorldToScreenPoint(transform.position + sliderPosOffset);
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
        this.monsterData = newMonsterData;
    }
}