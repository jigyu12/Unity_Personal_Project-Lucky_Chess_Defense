using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using UnityEngine.Rendering;

public class Hero : MonoBehaviour
{
    [SerializeField] private HeroData heroData;

    private IObjectPool<Hero> heroPool;

    private readonly UnityEvent<Monster, int> OnAttack = new();
    private IAttackMethod attackMethod;
    private RangedAttack rangedAttack;

    [SerializeField] private LayerMask monsterLayer;
    private ContactFilter2D contactFilter;
    private readonly List<Collider2D> monsterCollList = new();

    private Monster targetMonster;
    [SerializeField] private GameObject heroProjectilePrefab;
    private WaveManager waveManager;

    private float attackSpeedTimeAccum;
    
    public int HeroId => heroData.HeroID;
    public HeroGrade HeroGrade => heroData.Grade;
    public int Cost => heroData.BlockCost;
    public float AttackRange => heroData.AtkRange;
    public int SaleType => heroData.SaleType;
    public int SaleQuantity => heroData.SaleQuantity;
    
    public bool IsMoving { get; set; }
    public Vector3 destPosition;
    public const float HeroSwapSpeed = 5f;

    private GameObject parent;

    private HeroSpumSpawner heroSpumSpawner;
    private GameObject spumHeroGo;
    private SPUM_Prefabs spumPrefabs;

    private SortingGroup sortingGroup;
    
    private void Awake()
    {
        contactFilter.layerMask = monsterLayer;
        contactFilter.useLayerMask = true;
        contactFilter.useTriggers = true;
        
        attackSpeedTimeAccum = 0f;
        
        IsMoving = false;
        destPosition = new(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
    }
    
    private void OnDisable()
    {
        OnAttack.RemoveAllListeners();
        
        monsterCollList.Clear();
        
        targetMonster = null;

        if (spumHeroGo is not null)
        {
            heroSpumSpawner?.heroSpumPoolDict[heroData.HeroID].Release(spumHeroGo);
            spumHeroGo = null;
        }
    }
    
    private void Start()
    {
        parent = GameObject.FindGameObjectWithTag("Heroes");
        transform.SetParent(parent.transform);
        
        GameObject.FindGameObjectWithTag("WaveManager").TryGetComponent(out waveManager);
        monsterCollList.Capacity = waveManager.MaxMonsterCount + 5;
    }

    private void Update()
    {
        attackSpeedTimeAccum += Time.deltaTime;

        if (attackSpeedTimeAccum >= heroData.AtkSpeed)
        {
            bool canAttack = CheckCanAttack();

            if (canAttack)
                Attack();
        }

        MoveToCurrCellPos();
    }

    /// <summary>
    /// Call this method after the SetHeroDataByRandData function is invoked.
    /// </summary>
    public void Initialize()
    {
        if (heroData.AtkType == HeroAttackType.Melee)
            attackMethod = new MeleeAttack();
        else if (heroData.AtkType == HeroAttackType.Ranged)
        {
            TryGetComponent(out rangedAttack);

            attackMethod = rangedAttack;

            rangedAttack.Init(heroProjectilePrefab, this);
        }
        else
            Debug.Assert(false, "Invalid AttackMethod value");

        OnAttack.AddListener(attackMethod.Attack);

        attackSpeedTimeAccum = heroData.AtkSpeed;

        GameObject.FindGameObjectWithTag("HeroSpumSpawner").TryGetComponent(out heroSpumSpawner);
        spumHeroGo = heroSpumSpawner.heroSpumPoolDict[heroData.HeroID].Get();
        spumHeroGo.transform.SetParent(transform);
        spumHeroGo.transform.localPosition = Vector3.zero;
        
        spumHeroGo.TryGetComponent(out spumPrefabs);
        spumPrefabs.OverrideControllerInit();

        spumHeroGo.transform.GetChild(0).gameObject.TryGetComponent(out sortingGroup);
    }

    private bool CheckCanAttack()
    {
        if (IsMoving)
            return false;
        
        bool isTargetInvalid = IsTargetInvalid();
        
        if (isTargetInvalid)
        {
            monsterCollList.Clear();
            Physics2D.OverlapCircle(transform.position, heroData.AtkRange, contactFilter, monsterCollList);

            monsterCollList.RemoveAll(coll => Vector3.Distance(transform.position, coll.gameObject.transform.position) > heroData.AtkRange);
            
            if (monsterCollList.Count <= 0)
                return false;

            monsterCollList.Sort(MonsterAttackPriorityCmp);
            targetMonster = waveManager.CurrMonstersDict[monsterCollList[0]];
        }

        return true;
    }

    private bool IsTargetInvalid()
    {
        if (targetMonster is null)
            return true;

        if (targetMonster.IsDestroyed() || targetMonster.IsDead)
        {
            targetMonster = null;
            
            return true;
        }

        float distanceToTarget = Vector3.Distance(transform.position, targetMonster.transform.position);

        if (distanceToTarget > heroData.AtkRange)
        {
            targetMonster = null;
            
            return true;
        }

        return false;
    }

    private void Attack()
    {
        attackSpeedTimeAccum = 0f;

        spumPrefabs.PlayAnimation(PlayerState.ATTACK, 0);

        var localScale = transform.localScale;
        localScale.x = targetMonster.transform.position.x <= transform.position.x? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
        transform.localScale = localScale;
        
        StartCoroutine(OnAttackCoroutine());
    }

    private IEnumerator OnAttackCoroutine()
    {
        yield return null;
    
        while (true)
        {
            if (IsMoving)
                break;
            
            var stateInfo = spumPrefabs._anim.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.normalizedTime >= 0.5f)
            {
                OnAttack?.Invoke(targetMonster, heroData.HeroDamage);
                break;
            }
            yield return null;
        }
    }

    public void SetPool(IObjectPool<Hero> pool)
    {
        heroPool = pool;
    }

    public void DestroyHero()
    {
        heroPool.Release(this);
    }

    private int MonsterAttackPriorityCmp(Collider2D coll1, Collider2D coll2)
    {
        int cmp1 = ((int)waveManager.CurrMonstersDict[coll2].Type).CompareTo(
            (int)waveManager.CurrMonstersDict[coll1].Type);

        if (cmp1 != 0)
            return cmp1;

        return Vector3.Distance(transform.position, coll1.transform.position)
            .CompareTo(Vector3.Distance(transform.position, coll2.transform.position));
    }

    public void SetHeroData(HeroData setHeroData)
    {
        heroData = setHeroData;
    }
    
    private void MoveToCurrCellPos()
    {
        if(!IsMoving)
            return;
        
        transform.position = Vector3.MoveTowards(transform.position, destPosition, HeroSwapSpeed * Time.deltaTime);
        
        if (Vector3.Distance(transform.position, destPosition) < 0.01f)
        {
            IsMoving = false;

            transform.position = destPosition;
            
            spumPrefabs.PlayAnimation(PlayerState.IDLE, 0);
        }
    }
    
    public void SetHeroAnimMove(bool? heroAnimFlipVal)
    {
        var localScale = transform.localScale;
        
        if(heroAnimFlipVal is not null)
            localScale.x = (bool)heroAnimFlipVal ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
        
        transform.localScale = localScale;
        
        spumPrefabs.PlayAnimation(PlayerState.MOVE, 0);
    }

    public void SetHeroDrawOrder(int drawOrder)
    {
        sortingGroup.sortingOrder = drawOrder;
    }
    
#if UNITY_STANDALONE || UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!DebugModeUI.IsDebugMode)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, heroData.AtkRange);
    }
#endif
}