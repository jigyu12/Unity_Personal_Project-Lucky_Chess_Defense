using System.Collections.Generic;
using UnityEngine;

public class HeroSpawnPointInCell : MonoBehaviour
{
    public GameObject singlePos;
    
    public GameObject doublePos0;
    public GameObject doublePos1;
    
    public GameObject triplePos0;
    public GameObject triplePos1;
    public GameObject triplePos2;

    public int Cost { get; set; }
    public int HeroCount { get; set; }
    
    public int OccupyHeroId { get; set; }

    public List<Hero> heroList;

    private const int heroCountMax = 3;
    
    private void OnEnable()
    {
        Cost = 3;
        HeroCount = 0;
        OccupyHeroId = -1;
        
        heroList = new List<Hero>();
    }

    public bool CanSpawnHero(Hero hero)
    {
        if (hero == null || hero.heroData == null)
            return false;

        if (OccupyHeroId == -1)
        {
            return CalculatePlaceHeroByCostAndCount(hero);
        }
        else
        {
            if (OccupyHeroId != hero.heroData.HeroId)
                return false;
            
            return CalculatePlaceHeroByCostAndCount(hero);
        }
    }

    private bool CalculatePlaceHeroByCostAndCount(Hero hero)
    {
        if (Cost >= hero.heroData.Cost)
        {
            if(HeroCount >= heroCountMax)
                return false;
                
            OccupyHeroId = hero.heroData.HeroId;
            Cost -= hero.heroData.Cost;
                
            heroList.Add(hero);
            HeroCount++;

            PlaceHero(hero);
                
            return true;
        }
            
        return false;
    }

    private void PlaceHero(Hero hero)
    {
        switch (HeroCount)
        {
            case 1:
            {
                hero.transform.position = singlePos.transform.position;
            }
                break;
            case 2:
            {
                heroList[0].transform.position = doublePos0.transform.position;
                heroList[1].transform.position = doublePos1.transform.position;
            }
                break; 
            case 3:
            {
                heroList[0].transform.position = triplePos0.transform.position;
                heroList[1].transform.position = triplePos1.transform.position;
                heroList[2].transform.position = triplePos2.transform.position;
            }
                break;
            default:
                Debug.Assert(false, "Invaild Hero Count");
                break;
        }
    }
}