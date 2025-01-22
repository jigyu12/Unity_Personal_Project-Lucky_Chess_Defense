using System;
using UnityEngine;
using UnityEngine.Pool;

public class Hero : MonoBehaviour
{
    public HeroData heroData;
    
    private IObjectPool<Hero> heroPool;
    
    
    
    public void SetPool(IObjectPool<Hero> pool)
    {
        heroPool = pool;
    }

    public void DestroyHero()
    {
        heroPool.Release(this);
    }
}