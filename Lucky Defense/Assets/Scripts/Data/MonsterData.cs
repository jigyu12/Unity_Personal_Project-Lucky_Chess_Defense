using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "Scriptable Object/MonsterData")]
public class MonsterData : ScriptableObject
{
    [SerializeField] private int monsterID;

    public int MonsterID => monsterID;
    
    [SerializeField] private int stringKey;

    public int StringKey => stringKey;
    
    [SerializeField] private MonsterType monType;

    public MonsterType MonType => monType;
    
    [SerializeField] private int monHp;

    public int MonHp => monHp;
    
    [SerializeField] private float monSpeed;

    public float MonSpeed => monSpeed;
    
    [SerializeField] private int monReward;

    public int MonReward => monReward;
    
    [SerializeField] private int monQuantity;

    public int MonQuantity => monQuantity;
}

public class MonsterCsvData
{
    public int MonsterID { get; set; }
    public int StringKey { get; set; }
    public int MonType { get; set; }
    public int MonHp { get; set; }
    public float MonSpeed { get; set; }
    public int MonReward { get; set; }
    public int MonQuantity { get; set; }
}