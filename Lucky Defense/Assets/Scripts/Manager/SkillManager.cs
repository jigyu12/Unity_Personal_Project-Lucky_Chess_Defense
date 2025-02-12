using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SkillManager : MonoBehaviour
{
    public Dictionary<int, PassiveSkillData> AllPassiveSkillDict { get; } = new();
    public Dictionary<int, StatValueData> statValuePassiveSkillDict { get; } = new();
    public Dictionary<int, StatRateData> statRatePassiveSkillDict { get; } = new();
    public Dictionary<int, UnityAction> attackPassiveSkillDict { get; } = new();
    public Dictionary<int, UnityAction<Monster>> attackSkillToMonDict { get; } = new();
    
    [SerializeField] private InGameResourceManager inGameResourceManager;
    
    private void Awake()
    {
        AllPassiveSkillDict.Clear();
        statValuePassiveSkillDict.Clear();
        statRatePassiveSkillDict.Clear();
        attackPassiveSkillDict.Clear();

        foreach (var passiveSkillData in passiveSkillDataList)
        {
            AllPassiveSkillDict.Add(passiveSkillData.SkillID, passiveSkillData);
            
            if ((SkillEffectType)passiveSkillData.EffectType is
                SkillEffectType.AtkValue or SkillEffectType.AtkSpeedValue)
            {
                statValuePassiveSkillDict.Add(passiveSkillData.SkillID, CreateStatValueData(passiveSkillData));
            }
            else if ((SkillEffectType)passiveSkillData.EffectType is
                     SkillEffectType.AtkRate or SkillEffectType.AtkSpeedRate)
            {
                statRatePassiveSkillDict.Add(passiveSkillData.SkillID, CreateStatRateData(passiveSkillData));
            }
            else if ((SkillEffectType)passiveSkillData.EffectType is
                     SkillEffectType.AcquireCoin or SkillEffectType.AcquireGem)
            {
                attackPassiveSkillDict.Add(passiveSkillData.SkillID, CreateAttackPassiveSkill(passiveSkillData));
            }
            else if ((SkillEffectType)passiveSkillData.EffectType is
                     SkillEffectType.SpeedValue or SkillEffectType.SpeedRate)
            {
                attackSkillToMonDict.Add(passiveSkillData.SkillID, CreateAttackSkillToMon(passiveSkillData));
            }
            else
                Debug.Assert(false,
                    $"PassiveSkillData.EffectType : {passiveSkillData.EffectType} is invalid effectType value");
        }
    }

    private StatValueData CreateStatValueData(PassiveSkillData passiveSkillData)
    {
        StatValueData newStatValueData;
        
        int skillTypeVal = 0;
        if ((SkillType)passiveSkillData.SkillType == SkillType.Buff)
            skillTypeVal = 1;
        else if ((SkillType)passiveSkillData.SkillType == SkillType.Debuff)
            skillTypeVal = -1;

        if ((SkillEffectType)passiveSkillData.EffectType is SkillEffectType.AtkValue)
        {
            newStatValueData = new StatValueData(passiveSkillData.Value * skillTypeVal, 0f);

            return newStatValueData;
        }
        else if ((SkillEffectType)passiveSkillData.EffectType is SkillEffectType.AtkSpeedValue)
        {
            newStatValueData = new StatValueData(0f, passiveSkillData.Value * skillTypeVal);

            return newStatValueData;
        }

        Debug.Assert(false, "CreateStatValueData Failed");

        return null;
    }

    private StatRateData CreateStatRateData(PassiveSkillData passiveSkillData)
    {
        StatRateData newStatRateData;
        
        int skillTypeVal = 0;
        if ((SkillType)passiveSkillData.SkillType == SkillType.Buff)
            skillTypeVal = 1;
        else if ((SkillType)passiveSkillData.SkillType == SkillType.Debuff)
            skillTypeVal = -1;

        if ((SkillEffectType)passiveSkillData.EffectType is SkillEffectType.AtkRate)
        {
            newStatRateData = new StatRateData(passiveSkillData.Value * skillTypeVal, 0f);

            return newStatRateData;
        }
        else if ((SkillEffectType)passiveSkillData.EffectType is SkillEffectType.AtkSpeedRate)
        {
            newStatRateData = new StatRateData(0f, passiveSkillData.Value * skillTypeVal);

            return newStatRateData;
        }

        Debug.Assert(false, "CreateStatRateData Failed");

        return null;
    }

    private UnityAction CreateAttackPassiveSkill(PassiveSkillData passiveSkillData)
    {
        PassiveSkillData newPassiveSkillData = passiveSkillData;
        
        if ((SkillEffectType)newPassiveSkillData.EffectType is SkillEffectType.AcquireCoin)
        {
            return () =>
            {
                if (Random.value <= newPassiveSkillData.Probability * 0.01f)
                {
                    inGameResourceManager.AddCoin((int)newPassiveSkillData.Value);
                    
                    Debug.Log("Get Coin : " + (int)newPassiveSkillData.Value);
                }
            };
        }
        else if ((SkillEffectType)newPassiveSkillData.EffectType is SkillEffectType.AcquireGem)
        {
            return () =>
            {
                if (Random.value <= newPassiveSkillData.Probability * 0.01f)
                {
                    inGameResourceManager.AddGem((int)newPassiveSkillData.Value);
                    
                    Debug.Log("Get Gem : " + (int)newPassiveSkillData.Value);
                }
            };
        }

        Debug.Assert(false, "CreateAttackPassiveSkill Failed");
        
        return null;
    }

    private UnityAction<Monster> CreateAttackSkillToMon(PassiveSkillData passiveSkillData)
    {
        PassiveSkillData newPassiveSkillData = passiveSkillData;
        
        if ((SkillEffectType)newPassiveSkillData.EffectType is SkillEffectType.SpeedValue)
        {
            return (monster) => monster.MoveSpeedTestValue(newPassiveSkillData.Value);
        }
        else if ((SkillEffectType)newPassiveSkillData.EffectType is SkillEffectType.SpeedRate)
        {
            return (monster) => monster.MoveSpeedTestRate(newPassiveSkillData.Value);
        }
        
        Debug.Assert(false, "CreateAttackSkillToMon Failed");
        
        return null;
    }
    

    public List<PassiveSkillData> passiveSkillDataList = new();
}