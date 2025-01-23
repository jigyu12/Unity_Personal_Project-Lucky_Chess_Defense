using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Monster : MonoBehaviour
{
    public MonsterData monsterData;
    
    public List<Vector3> waypoint;
    public int currentWaypointIndex;
    
    private IObjectPool<Monster> monsterPool;
    
    private void OnEnable()
    {
        currentWaypointIndex = 0;
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, waypoint[currentWaypointIndex], monsterData.Speed * Time.deltaTime);
        
        if (Vector3.Distance(transform.position, waypoint[currentWaypointIndex]) < 0.01f)
        {
            transform.position = waypoint[currentWaypointIndex];
            
            currentWaypointIndex = ++currentWaypointIndex % waypoint.Count;
        }
    }

    public void SetPool(IObjectPool<Monster> pool)
    {
        monsterPool = pool;
    }

    public void DestroyMonster()
    {
        monsterPool.Release(this);
    }
}