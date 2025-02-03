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
    
    private void Start()
    {
        parent = GameObject.FindGameObjectWithTag("Projectiles");
        transform.SetParent(parent.transform);
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

        Quaternion lookRotation = Quaternion.LookRotation(targetLastPosition - projectilePosition);
        lookRotation.x = 0f; 
        lookRotation.y = 0f;

        transform.SetPositionAndRotation(movePosition, lookRotation);
    }

    public void Initialize(Monster targetMon, int dmg, Vector3 firePosition)
    {
        targetMonster = targetMon;
        damage = dmg;
        
        SetTargetLastPosAndDir(firePosition);
        
        Quaternion lookRotation = Quaternion.LookRotation(targetLastPosition - firePosition);
        
        transform.SetPositionAndRotation(firePosition, lookRotation);
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