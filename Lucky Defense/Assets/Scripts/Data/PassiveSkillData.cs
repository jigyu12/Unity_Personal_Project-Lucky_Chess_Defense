using UnityEngine;

[CreateAssetMenu(fileName = "PassiveSkillData", menuName = "Scriptable Object/PassiveSkillData")]
public class PassiveSkillData : ScriptableObject
{
    [SerializeField] private int skillID;

    public int SkillID => skillID;
    
    [SerializeField] private int skillType;

    public int SkillType => skillType;
    
    [SerializeField] private int targetType;

    public int TargetType => targetType;
    
    [SerializeField] private int effectType;

    public int EffectType => effectType;
    
    [SerializeField] private float probability;
    
    public float Probability => probability;
    
    [SerializeField] private float value;
    
    public float Value => value;
    
    [SerializeField] private float duration;
    
    public float Duration => duration;
}

public class PassiveSkillCsvData
{
    public int SkillID { get; set; }
    public int SkillType { get; set; }
    public int TargetType { get; set; }
    public int EffectType { get; set; }
    public float Probability { get; set; }
    public float Value { get; set; }
    public float Duration { get; set; }
}