using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "Scriptable Object/MonsterData")]
public class MonsterData : ScriptableObject
{
    [SerializeField] private int id;

    public int Id
    {
        get { return id; }
    }
    
    [SerializeField] private MonsterType type;

    public MonsterType Type
    {
        get { return type; }
    }
    
    [SerializeField] private int hp;

    public int Hp
    {
        get { return hp; }
    }
    
    [SerializeField] private float speed;

    public float Speed
    {
        get { return speed; }
    }
    
    [SerializeField] private int rewardType;

    public int RewardType
    {
        get { return rewardType; }
    }
    
    [SerializeField] private int quantity;

    public int Quantity
    {
        get { return quantity; }
    }
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