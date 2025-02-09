using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class HeroProjectile : MonoBehaviour
{
    private IObjectPool<HeroProjectile> heroProjectilePool;
    
    private const float ProjectileSpeed = 8f;
    
    private Vector3 targetLastPosition;
    private Vector3 direction;
    
    private Monster targetMonster;
    private int damage;
    
    private GameObject parent;
    
    private HeroProjectileSpawner heroProjectileSpawner;
    
    private HeroGrade ownerHeroGrade;
    private GameObject heroFireProjectile;
    
    private void Start()
    {
        parent = GameObject.FindGameObjectWithTag("Projectiles");
        transform.SetParent(parent.transform);

        GameObject.FindGameObjectWithTag("HeroProjectileSpawner").TryGetComponent(out heroProjectileSpawner);
        heroFireProjectile = heroProjectileSpawner.heroProjectilePoolDict[ownerHeroGrade].Get();
        heroFireProjectile.transform.SetParent(transform);
        heroFireProjectile.transform.localPosition = Vector3.zero;
    }

    private void Update()
    {
        bool isTargetValid = targetMonster is not null && !targetMonster.IsDestroyed() && !targetMonster.IsDead;

        Vector3 projectilePosition = transform.position;
        
        if (isTargetValid)
            SetTargetLastPosAndDir(projectilePosition);
        else
            targetMonster = null;
        
        if (Vector3.Distance(projectilePosition, targetLastPosition) < 0.5f)
        {
            if(isTargetValid)
                targetMonster.OnDamaged(damage);
            
            DestroyHeroProjectile();
            
            return;
        }
        
        Vector3 movePosition = projectilePosition + direction * (ProjectileSpeed * Time.deltaTime);
    
        Quaternion baseRotation = Quaternion.LookRotation(direction);
    
        float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        if (angle < 0f)
            angle += 360f;
    
        float additionalAngle = angle < 180f ? 90f : -90f;
    
        Quaternion finalRotation = baseRotation * Quaternion.Euler(0f, 0f, additionalAngle);
        finalRotation.x = 0f;
        finalRotation.y = 0f;
    
        transform.SetPositionAndRotation(movePosition, finalRotation);
    
        heroFireProjectile.transform.localRotation = Quaternion.identity;
        
        // Quaternion rotation = transform.rotation;
        // rotation.x = 0f; 
        // rotation.y = 0f;
        //
        // transform.SetPositionAndRotation(movePosition, rotation);
        //
        // heroFireProjectile.transform.localRotation = rotation;
    }

    public void Initialize(Monster targetMon, int dmg, Vector3 firePosition, HeroGrade heroGrade)
    {
        targetMonster = targetMon;
        damage = dmg;
        
        SetTargetLastPosAndDir(firePosition);
        
        Quaternion rotation = transform.rotation;
        rotation.x = 0f; 
        rotation.y = 0f;
        
        transform.SetPositionAndRotation(firePosition, rotation);
        
        ownerHeroGrade = heroGrade;
    }

    private void SetTargetLastPosAndDir(Vector3 currObjPos)
    {
        targetLastPosition = targetMonster.transform.position;
        direction = (targetLastPosition - currObjPos).normalized;
    }
    
    public void SetPool(IObjectPool<HeroProjectile> pool)
    {
        heroProjectilePool = pool;
    }
    
    private void DestroyHeroProjectile()
    {
        heroProjectilePool.Release(this);
    }
}