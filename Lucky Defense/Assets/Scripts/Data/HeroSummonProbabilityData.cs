using UnityEngine;

[CreateAssetMenu(fileName = "HeroSummonProbabilityData", menuName = "Scriptable Object/HeroSummonProbabilityData")]
public class HeroSummonProbabilityData : ScriptableObject
{
    [SerializeField] private int probabilityID;

    public int ProbabilityID => probabilityID;
    
    [SerializeField] private int enforceLevel;

    public int EnforceLevel => enforceLevel;
    
    [SerializeField] private int enforceCost;

    public int EnforceCost => enforceCost;
    
    [SerializeField] private float commonProbability;

    public float CommonProbability => commonProbability;
    
    [SerializeField] private float rareProbability;

    public float RareProbability => rareProbability;
    
    [SerializeField] private float heroicProbability;

    public float HeroicProbability => heroicProbability;
    
    [SerializeField] private float legendaryProbability;

    public float LegendaryProbability => legendaryProbability;
}

public class HeroSummonProbabilityCsvData
{
    public int ProbabilityID { get; set; }
    public int EnforceLevel { get; set; }
    public int EnforceCost { get; set; }
    public float CommonProbability { get; set; }
    public float RareProbability { get; set; }
    public float HeroicProbability { get; set; }
    public float LegendaryProbability { get; set; }
}