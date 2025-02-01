using UnityEngine;

[CreateAssetMenu(fileName = "WaveData", menuName = "Scriptable Object/WaveData")]
public class WaveData : ScriptableObject
{
    [SerializeField] private int id;

    public int Id => id;
    
    [SerializeField] private int waveNumber;

    public int WaveNumber => waveNumber;
    
    [SerializeField] private int spawnMonsterId;

    public int SpawnMonsterId => spawnMonsterId;
    
    [SerializeField] private float hpMultiplier;

    public float HpMultiplier => hpMultiplier;
    
    [SerializeField] private int spawnMonsterCount;

    public int SpawnMonsterCount => spawnMonsterCount;
    
    [SerializeField] private int waveTime;

    public int WaveTime => waveTime;
}

public class WaveCsvData
{
    public int Id { get; set; }
    public int WaveNumber { get; set; }
    public int SpawnMonsterId { get; set; }
    public float HpMultiplier { get; set; }
    public int SpawnMonsterCount { get; set; }
    public int WaveTime { get; set; }
}