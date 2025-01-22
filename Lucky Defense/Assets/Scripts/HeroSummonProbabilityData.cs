using UnityEngine;

[CreateAssetMenu(fileName = "HeroSummonProbabilityData", menuName = "Scriptable Object/HeroSummonProbabilityData")]
public class HeroSummonProbabilityData : ScriptableObject
{
    [SerializeField] private int id;

    public float Id
    {
        get { return id; }
    }
    
    [SerializeField] private float commonProbability;

    public float CommonProbability
    {
        get { return commonProbability; }
    }
    
    [SerializeField] private float rareProbability;

    public float RareProbability
    {
        get { return rareProbability; }
    }
    
    [SerializeField] private float heroicProbability;

    public float HeroicProbability
    {
        get { return heroicProbability; }
    }
    
    [SerializeField] private float legendaryProbability;

    public float LegendaryProbability
    {
        get { return legendaryProbability; }
    }
}

public class HeroSummonProbabilityCsvData
{
    public int Id { get; set; }
    public float CommonProbability { get; set; }
    public float RareProbability { get; set; }
    public float HeroicProbability { get; set; }
    public float LegendaryProbability { get; set; }
}