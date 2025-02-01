using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "Scriptable Object/MonsterData")]
public class MonsterData : ScriptableObject
{
    [SerializeField] private int id;

    public int Id => id;
    
    [SerializeField] private MonsterType type;

    public MonsterType Type => type;
    
    [SerializeField] private int hp;

    public int Hp => hp;
    
    [SerializeField] private float speed;

    public float Speed => speed;
    
    [SerializeField] private int rewardType;

    public int RewardType => rewardType;
    
    [SerializeField] private int quantity;

    public int Quantity => quantity;
}

public class MonsterCsvData
{
    public int Id { get; set; }
    public int Type { get; set; }
    public int Hp { get; set; }
    public float Speed { get; set; }
    public int RewardType { get; set; }
    public int Quantity { get; set; }
}