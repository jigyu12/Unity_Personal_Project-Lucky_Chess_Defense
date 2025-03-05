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

    private readonly UnityEvent<Monster, int, UnityAction<Monster>> OnAttack = new();
    private IAttackMethod attackMethod;
    private RangedAttack rangedAttack;

    [SerializeField] private LayerMask monsterLayer;
    private ContactFilter2D contactFilter;
    private readonly List<Collider2D> monsterCollList = new();

    private Monster targetMonster;
    private bool isAttacking;
    [SerializeField] private GameObject heroProjectilePrefab;
    private WaveManager waveManager;

    private float attackSpeedTimeAccum;
    
    public int HeroId => heroData.HeroID;
    public HeroGrade HeroGrade => heroData.Grade;
    public int Cost => heroData.BlockCost;
    public float AttackRange => heroData.AtkRange;
    public int SaleType => heroData.SaleType;
    public int SaleQuantity => heroData.SaleQuantity;
    public int SynergyClass1 => heroData.SynergyClass1;
    
    public bool IsMoving { get; set; }
    public Vector3 destPosition;
    public const float HeroSwapSpeed = 5f;

    private GameObject parent;

    private HeroSpumSpawner heroSpumSpawner;
    private GameObject spumHeroGo;
    private SPUM_Prefabs spumPrefabs;

    private SortingGroup sortingGroup;

    public List<StatValueData> statValueDataList;
    public List<StatRateData> statRateDataList;
    public UnityAction OnAdditionalAttack;
    public UnityAction<Monster> OnAttackToMon;
    
    private int additionalAtkValue;
    private float additionalAtkRate;
    private int additionalAtkSpeedValue;   
    private float additionalAtkSpeedRate;
    
    private HeroInfoData heroInfoData; 
    
    private void Awake()
    {
        contactFilter.layerMask = monsterLayer;
        contactFilter.useLayerMask = true;
        contactFilter.useTriggers = true;
        
        attackSpeedTimeAccum = 0f;
        
        IsMoving = false;
        destPosition = new(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);

        statValueDataList = new();
        statRateDataList = new();
        OnAdditionalAttack = null;

        additionalAtkValue = 0;
        additionalAtkRate = 0f;
        additionalAtkSpeedValue = 0;
        additionalAtkSpeedRate = 0f;

        isAttacking = false;

        heroInfoData = new();
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

        if (attackSpeedTimeAccum >= Mathf.Max(0.1f, (heroData.AtkSpeed - additionalAtkSpeedValue)  - (heroData.AtkSpeed * additionalAtkSpeedRate)))
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
        
        if(heroSpumSpawner is null)
            GameObject.FindGameObjectWithTag("HeroSpumSpawner").TryGetComponent(out heroSpumSpawner);
        
        spumHeroGo = heroSpumSpawner.heroSpumPoolDict[heroData.HeroID].Get();
        spumHeroGo.transform.SetParent(transform);
        spumHeroGo.transform.localPosition = Vector3.zero;
        var localScale = spumHeroGo.transform.localScale;
        localScale.x = Mathf.Abs(localScale.x);
        spumHeroGo.transform.localScale = localScale;
        
        spumHeroGo.TryGetComponent(out spumPrefabs);
        spumPrefabs.OverrideControllerInit();

        spumHeroGo.transform.GetChild(0).gameObject.TryGetComponent(out sortingGroup);
        
        isAttacking = false;
    }

    private bool CheckCanAttack()
    {
        if (IsMoving || isAttacking)
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
            
            // Physics2D.OverlapCircle(transform.position, heroData.AtkRange, contactFilter, monsterCollList);
            //
            // if (monsterCollList.Count <= 0)
            //     return false;
            //
            // monsterCollList.Sort(MonsterAttackPriorityCmp);
            // targetMonster = waveManager.CurrMonstersDict[monsterCollList[0]];
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

        isAttacking = true;
        
        float originAtkSpeed = heroData.AtkSpeed;
        float currAtkSpeed = heroData.AtkSpeed - (heroData.AtkSpeed * additionalAtkSpeedRate);
        float attackAnimSpeed = originAtkSpeed / currAtkSpeed;
        spumPrefabs._anim.speed = attackAnimSpeed;
        
        spumPrefabs.PlayAnimation(PlayerState.ATTACK, 0);

        var localScale = transform.localScale;
        localScale.x = targetMonster.transform.position.x <= transform.position.x? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
        transform.localScale = localScale;
        
        StartCoroutine(OnAttackCoroutine());
    }

    private IEnumerator OnAttackCoroutine()
    {
        yield return null;
        
        bool hasAttacked = false;
     
        while (true)
        {
            if (IsMoving)
            {
                isAttacking = false;

                spumPrefabs._anim.speed = 1f;
                
                yield break;
            }
            
            var stateInfo = spumPrefabs._anim.GetCurrentAnimatorStateInfo(0);
            if (!hasAttacked && stateInfo.normalizedTime >= 0.5f)
            {
                int heroDamage = (heroData.HeroDamage + additionalAtkValue) + (int)(heroData.HeroDamage * additionalAtkRate);

                if (Random.value <= heroData.CriticalPercent)
                    heroDamage = (int)(heroDamage * heroData.CriticalMlt);
                
                if (heroData.AtkType == HeroAttackType.Melee)
                    SoundManager.Instance.PlaySfx(SfxClipId.MeleeAttackSfxSoundId);
                else if (heroData.AtkType == HeroAttackType.Ranged)
                    SoundManager.Instance.PlaySfx(SfxClipId.RangedAttackSfxSoundId);
                
                OnAttack?.Invoke(targetMonster, heroDamage, OnAttackToMon);
                OnAdditionalAttack?.Invoke();

                isAttacking = false;
                hasAttacked = true;
            }

            if (stateInfo.normalizedTime >= 1f)
            {
                spumPrefabs._anim.speed = 1f;
                
                yield break;
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

        return Vector2.Distance(transform.position, coll1.transform.position)
            .CompareTo(Vector2.Distance(transform.position, coll2.transform.position));
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

    public void SetValueDataList(StatValueData statValueData, bool isAddData)
    {
        if (isAddData)
        {
            if(!statValueDataList.Contains(statValueData))
                statValueDataList.Add(statValueData);
        }
        else
        {
            if(statValueDataList.Contains(statValueData))
                statValueDataList.Remove(statValueData);
        }
        
        additionalAtkValue = 0;
        additionalAtkSpeedValue = 0;
        foreach (var data in statValueDataList)
        {
            additionalAtkValue += (int)data.AtkDamageValue;
            additionalAtkSpeedValue += (int)data.AtkDamageValue;
        }
    }

    public void SetRateDataList(StatRateData statRateData, bool isAddData)
    {
        if (isAddData)
        {
            if(!statRateDataList.Contains(statRateData))
                statRateDataList.Add(statRateData);
        }
        else
        {
            if(statRateDataList.Contains(statRateData))
                statRateDataList.Remove(statRateData);
        }

        additionalAtkRate = 0;
        additionalAtkSpeedRate = 0;
        foreach (var data in statRateDataList)
        {
            additionalAtkRate += data.AtkDamageRate;
            additionalAtkSpeedRate += data.AtkSpeedRate;
        }
    }

    public void SetOnAdditionalAttack(UnityAction additionalAttack, bool isAddAction)
    {
        bool isExist = OnAdditionalAttack != null &&
                            OnAdditionalAttack.GetInvocationList().Any(d =>
                                d.Method == (additionalAttack).Method &&
                                d.Target == (additionalAttack).Target);

        if (isAddAction)
        {
            if (!isExist)
                OnAdditionalAttack += additionalAttack;
        }
        else
        {
            if(isExist)
                OnAdditionalAttack -= additionalAttack;
        }
    }
    
    public void SetOnAttackToMon(UnityAction<Monster> onAttackToMon, bool isAddAction)
    {
        bool isExist = OnAttackToMon != null &&
                       OnAttackToMon.GetInvocationList().Any(d =>
                           d.Method == (onAttackToMon).Method &&
                           d.Target == (onAttackToMon).Target);

        if (isAddAction)
        {
            if (!isExist)
                OnAttackToMon += onAttackToMon;
        }
        else
        {
            if(isExist)
                OnAttackToMon -= onAttackToMon;
        }
    }

    public HeroInfoData GetCurrHeroInfoData()
    {
        heroInfoData.HeroID = HeroId;
        heroInfoData.HeroGrade = HeroGrade;
        heroInfoData.HeroSynergyClass = SynergyClass1;
        heroInfoData.AtkType = heroData.AtkType;
        heroInfoData.AtkDamage = (heroData.HeroDamage + additionalAtkValue) + (int)(heroData.HeroDamage * additionalAtkRate);
        heroInfoData.AtkSpeed = (heroData.AtkSpeed - additionalAtkSpeedValue)  - (heroData.AtkSpeed * additionalAtkSpeedRate);
        heroInfoData.CriticalRate = heroData.CriticalPercent;
        heroInfoData.CriticalMlt = heroData.CriticalMlt;
        
        return heroInfoData;
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