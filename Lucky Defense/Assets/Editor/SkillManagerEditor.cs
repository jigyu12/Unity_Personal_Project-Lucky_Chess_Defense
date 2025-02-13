using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SkillManager))]
public class SkillManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        SkillManager manager = (SkillManager)target;

        if (GUILayout.Button("Load SkillData CSV"))
        {
            LoadCsvAndCreateSkillDataScriptables(manager);
        }

        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
        }
    }

    private void LoadCsvAndCreateSkillDataScriptables(SkillManager manager)
    {
        string path = EditorUtility.OpenFilePanel("Load Skill Data CSV", "Assets/Resources/Tables", "csv");
        if (string.IsNullOrEmpty(path)) 
            return;
        
        manager.passiveSkillDataList.Clear();
        
        using (var reader = new StreamReader(path))
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            var records = csv.GetRecords<PassiveSkillCsvData>();

            foreach (var record in records)
            {
                PassiveSkillData passiveData = ScriptableObject.CreateInstance<PassiveSkillData>();
                passiveData.name = $"PassiveSkillData_{record.SkillID}";
                
                typeof(PassiveSkillData)
                    .GetField("skillID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(passiveData, record.SkillID);
                typeof(PassiveSkillData)
                    .GetField("skillType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(passiveData, record.SkillType);
                typeof(PassiveSkillData)
                    .GetField("targetType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(passiveData, record.TargetType);
                typeof(PassiveSkillData)
                    .GetField("effectType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(passiveData, record.EffectType);
                typeof(PassiveSkillData)
                    .GetField("probability", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(passiveData, record.Probability);
                typeof(PassiveSkillData)
                    .GetField("value", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(passiveData, record.Value);
                typeof(PassiveSkillData)
                    .GetField("duration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(passiveData, record.Duration);
                
                string assetPath = $"Assets/Scriptables/SkillDatas/SkillData_{record.SkillID}.asset";
                Directory.CreateDirectory("Assets/Scriptables/SkillDatas");
                AssetDatabase.CreateAsset(passiveData, assetPath);

                manager.passiveSkillDataList.Add(passiveData);
            }
        }
        
        EditorUtility.SetDirty(manager);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}