using UnityEngine;

[CreateAssetMenu(fileName = "HeroData", menuName = "Scriptable Object/HeroData")]
public class HeroData : ScriptableObject
{
    [SerializeField] private int heroId;

    public int HeroId => heroId;
    
    [SerializeField] private float attackRange;

    public float AttackRange => attackRange;
    
    [SerializeField] private int damage;

    public int Damage => damage;
    
    [SerializeField] private HeroRarity rarity;
    
    public HeroRarity Rarity => rarity;
    
    [SerializeField] private int cost;
    
    public int Cost => cost;

    [SerializeField] private HeroAttackMethod attackMethod;

    public HeroAttackMethod AttackMethod => attackMethod;
    
    [SerializeField] private float attackSpeed;

    public float AttackSpeed => attackSpeed;
}

public class HeroCsvData
{
    public int HeroId { get; set; }
    public float AttackRange { get; set; }
    public int Damage { get; set; }
    public int Rarity { get; set; }
    public int Cost { get; set; }
    public HeroAttackMethod AttackMethod { get; set; }
    public float AttackSpeed  { get; set; }
}