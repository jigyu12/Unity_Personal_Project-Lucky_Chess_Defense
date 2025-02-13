using UnityEngine;

[CreateAssetMenu(fileName = "SynergyData", menuName = "Scriptable Object/SynergyData")]
public class SynergyData : ScriptableObject
{
    [SerializeField] private int synergyID;

    public int SynergyID => synergyID;
    
    [SerializeField] private int synergyName;

    public int SynergyName => synergyName;
    
    [SerializeField] private int synergyMainDescription;

    public int SynergyMainDescription => synergyMainDescription;
    
    [SerializeField] private int synergyEffectScript1;

    public int SynergyEffectScript1 => synergyEffectScript1;

    [SerializeField] private int synergyEffectScript2;

    public int SynergyEffectScript2 => synergyEffectScript2;
    
    [SerializeField] private int synergyEffectScript3;

    public int SynergyEffectScript3 => synergyEffectScript3;
    
    [SerializeField] private int synergyClassType;

    public int SynergyClassType => synergyClassType;
    
    [SerializeField] private int heroIDCount;

    public int HeroIDCount => heroIDCount;
    
    [SerializeField] private int synergySkill1;

    public int SynergySkill1 => synergySkill1;
    
    [SerializeField] private int synergySkill2;

    public int SynergySkill2 => synergySkill2;
}

public class SynergyCsvData
{
    public int SynergyID { get; set; }
    public int SynergyName { get; set; }
    public int SynergyMainDescription { get; set; }
    public int SynergyEffectScript1 { get; set; }
    public int SynergyEffectScript2 { get; set; }
    public int SynergyEffectScript3 { get; set; }
    public int SynergyClassType { get; set; }
    public int HeroIDCount { get; set; }
    public int SynergySkill1 { get; set; }
    public int SynergySkill2 { get; set; }
}