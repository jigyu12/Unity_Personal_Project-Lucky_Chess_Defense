using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SynergyManager : MonoBehaviour
{
    [SerializeField] private SkillManager skillManager;
    public Dictionary<int, Dictionary<int ,List<HeroSpawnPointInCell>>> SynergyCellListByOccupyHeroIdDict { get; } = new();
    private Dictionary<int, List<SynergyData>> SynergyDataListBySynergyClassDict { get; } = new();
    private List<int> skillIdList = new();

    public static UnityEvent<List<int>, bool, int> OnSynergyCalcualted = new();
    
    private void Start()
    {
        HeroSpawnPointInCell.OnOccupyHeroIdChangedInCell.AddListener(OnOccupyHeroIdChangedHandler);

        for (int i = 1; i <= Utility.SynergyClassCount; ++i)
        {
            SynergyCellListByOccupyHeroIdDict.Add(i, new Dictionary<int ,List<HeroSpawnPointInCell>>());
        }
        
        synergyList.Sort((a, b) =>
        {
            int cmpType = a.SynergyClassType.CompareTo(b.SynergyClassType);
            if(cmpType != 0)
                return cmpType;
            
            return a.HeroIDCount.CompareTo(b.HeroIDCount);
        });
        
        for (int i = 0; i < synergyList.Count; ++i)
        {
            if(!SynergyDataListBySynergyClassDict.ContainsKey(synergyList[i].SynergyClassType))
            {
                List<SynergyData> newSynergyDataList = new();
                newSynergyDataList.Add(synergyList[i]);
                
                SynergyDataListBySynergyClassDict.Add(synergyList[i].SynergyClassType, newSynergyDataList);
            }
            else
            {
                SynergyDataListBySynergyClassDict[synergyList[i].SynergyClassType].Add(synergyList[i]);
            }
        }
    }
 
    /// <summary>
    /// Call this function immediately before and immediately after HeroSpawnPointInCell’s OccupyHeroId changes.
    /// </summary>
    /// <param name="cell">Cell to be handled.</param>
    /// <param name="isAddCell">If this value is true, add the cell; if false, remove the cell.</param>
    private void OnOccupyHeroIdChangedHandler(HeroSpawnPointInCell cell, bool isAddCell)
    {
        int cellSynergyClass1 = cell.currCellSynergyClass;
        if (cellSynergyClass1 == HeroSpawnPointInCell.DefaultOccupyHeroId)
            return;
        
        if (isAddCell)
        {
            if (!SynergyCellListByOccupyHeroIdDict[cellSynergyClass1].ContainsKey(cell.OccupyHeroId))
            {
                List<HeroSpawnPointInCell> newCellList = new();
                newCellList.Add(cell);
                
                SynergyCellListByOccupyHeroIdDict[cellSynergyClass1].Add(cell.OccupyHeroId, newCellList);
            }
            else
            {
                SynergyCellListByOccupyHeroIdDict[cellSynergyClass1][cell.OccupyHeroId].Add(cell);
            }
        }
        else
        {
            SynergyCellListByOccupyHeroIdDict[cellSynergyClass1][cell.OccupyHeroId].Remove(cell);

            if (SynergyCellListByOccupyHeroIdDict[cellSynergyClass1][cell.OccupyHeroId].Count == 0)
            {
                SynergyCellListByOccupyHeroIdDict[cellSynergyClass1].Remove(cell.OccupyHeroId);
            }
        }

        var synergyDataList = SynergyDataListBySynergyClassDict[cellSynergyClass1];
        for (int i = -1; i < synergyDataList.Count; ++i)
        {
            if (i == -1)
            {
                if (0 <= SynergyCellListByOccupyHeroIdDict[cellSynergyClass1].Count 
                    && SynergyCellListByOccupyHeroIdDict[cellSynergyClass1].Count < synergyDataList[i + 1].HeroIDCount)
                {
                    if (!isAddCell)
                    {
                        SetSkillIdsInSkillIdList(synergyDataList[0]);
                        OnSynergyCalcualted?.Invoke(skillIdList, false, cellSynergyClass1);
                    }

                    break;
                }
            }
            else if (0 <= i && i < synergyDataList.Count - 1)
            {
                if (synergyDataList[i].HeroIDCount <= SynergyCellListByOccupyHeroIdDict[cellSynergyClass1].Count 
                    && SynergyCellListByOccupyHeroIdDict[cellSynergyClass1].Count < synergyDataList[i + 1].HeroIDCount)
                {

                    if (isAddCell)
                    {
                        SetSkillIdsInSkillIdList(synergyDataList[i]);
                        OnSynergyCalcualted?.Invoke(skillIdList, true, cellSynergyClass1);

                        if (i != 0)
                        {
                            SetSkillIdsInSkillIdList(synergyDataList[i-1]);
                            OnSynergyCalcualted?.Invoke(skillIdList, false, cellSynergyClass1);
                        }
                    }
                    else
                    {
                        SetSkillIdsInSkillIdList(synergyDataList[i]);
                        OnSynergyCalcualted?.Invoke(skillIdList, true, cellSynergyClass1);
                        
                        SetSkillIdsInSkillIdList(synergyDataList[i+1]);
                        OnSynergyCalcualted?.Invoke(skillIdList, false, cellSynergyClass1);
                    }

                    break;
                }
            }
            else
            {
                if (isAddCell)
                {
                    SetSkillIdsInSkillIdList(synergyDataList[i]);
                    OnSynergyCalcualted?.Invoke(skillIdList, true, cellSynergyClass1);
                        
                    SetSkillIdsInSkillIdList(synergyDataList[i-1]);
                    OnSynergyCalcualted?.Invoke(skillIdList, false, cellSynergyClass1);
                }

                break;
            }
        }
    }

    public void SetSkillIdsInSkillIdList(SynergyData synergyData)
    {
        skillIdList.Clear();
        
        if (synergyData is null)
            return;
        
        if(synergyData.SynergySkill1 != 0)
            skillIdList.Add(synergyData.SynergySkill1);
        
        if(synergyData.SynergySkill2 != 0)
            skillIdList.Add(synergyData.SynergySkill2);
    }
    
    public List<SynergyData> synergyList = new();
}