using UnityEngine;
using UnityEngine.Pool;

public class RangedAttack : MonoBehaviour, IAttackMethod
{
    private IObjectPool<HeroProjectile> heroProjectilePool;
    private GameObject heroProjectilePrefab;
    
    private Hero owner;

    public void Init(GameObject projectilePrefab, Hero newOwner)
    {
        heroProjectilePool = new ObjectPool<HeroProjectile>(OnCreateHeroProjectile, OnGetHeroProjectile, OnReleaseHeroProjectile, OnDestroyHeroProjectile);
        
        heroProjectilePrefab = projectilePrefab;
        
        owner = newOwner;
    }
    
    public void Attack(Monster targetMonster, int damage)
    {
        var projectile = heroProjectilePool.Get();
        
        projectile.Initialize(targetMonster, damage, owner.transform.position, owner.HeroGrade);
    }
    
    private HeroProjectile OnCreateHeroProjectile()
    {
        Instantiate(heroProjectilePrefab).TryGetComponent(out HeroProjectile heroProjectile);
        
        heroProjectile.SetPool(heroProjectilePool);
        
        return heroProjectile;
    }

    private void OnGetHeroProjectile(HeroProjectile heroProjectile)
    {
        heroProjectile.gameObject.SetActive(true);
    }

    private void OnReleaseHeroProjectile(HeroProjectile heroProjectile)
    {
        heroProjectile.gameObject.SetActive(false);
    }

    private void OnDestroyHeroProjectile(HeroProjectile heroProjectile)
    {
        Destroy(heroProjectile.gameObject);
    }
}