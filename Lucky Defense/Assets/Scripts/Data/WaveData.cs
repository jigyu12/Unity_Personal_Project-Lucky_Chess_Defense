using UnityEngine;

[CreateAssetMenu(fileName = "WaveData", menuName = "Scriptable Object/WaveData")]
public class WaveData : ScriptableObject
{
    [SerializeField] private int waveID;

    public int WaveID => waveID;
    
    [SerializeField] private int waveNumber;

    public int WaveNumber => waveNumber;
    
    [SerializeField] private int monsterID;

    public int MonsterID => monsterID;
    
    [SerializeField] private float monHpMlt;

    public float MonHpMlt => monHpMlt;
    
    [SerializeField] private int createMonNumber;

    public int CreateMonNumber => createMonNumber;
    
    [SerializeField] private int waveTime;

    public int WaveTime => waveTime;
}

public class WaveCsvData
{
    public int WaveID { get; set; }
    public int WaveNumber { get; set; }
    public int MonsterID { get; set; }
    public float MonHpMlt { get; set; }
    public int CreateMonNumber { get; set; }
    public int WaveTime { get; set; }
}