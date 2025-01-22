using UnityEngine;

[CreateAssetMenu(fileName = "HeroData", menuName = "Scriptable Object/HeroData")]
public class HeroData : ScriptableObject
{
    [SerializeField] private int heroId;

    public int HeroId
    {
        get { return heroId; }
    }
    
    [SerializeField] private float attackRange;

    public float AttackRange
    {
        get { return attackRange; }
    }
    
    [SerializeField] private int damage;

    public int Damage
    {
        get { return damage; }
    }
    
    [SerializeField] private HeroRarity rarity;
    
    public HeroRarity Rarity
    {
        get { return rarity; }
    }
    
    [SerializeField] private int cost;
    
    public int Cost
    {
        get { return cost; }
    }
}

public class HeroCsvData
{
    public int HeroId { get; set; }
    public float AttackRange { get; set; }
    public int Damage { get; set; }
    public int Rarity { get; set; }
    public int Cost { get; set; }
}