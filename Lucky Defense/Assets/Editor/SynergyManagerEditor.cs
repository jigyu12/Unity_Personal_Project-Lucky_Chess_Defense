using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SynergyManager))]
public class SynergyManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        SynergyManager manager = (SynergyManager)target;

        if (GUILayout.Button("Load SynergyData CSV"))
        {
            LoadCsvAndCreateSynergyDataScriptables(manager);
        }

        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
        }
    }

    private void LoadCsvAndCreateSynergyDataScriptables(SynergyManager manager)
    {
        string path = EditorUtility.OpenFilePanel("Load Synergy Data CSV", "Assets/Resources/Tables", "csv");
        if (string.IsNullOrEmpty(path)) 
            return;
        
        manager.synergyList.Clear();
        
        using (var reader = new StreamReader(path))
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            var records = csv.GetRecords<SynergyCsvData>();

            foreach (var record in records)
            {
                SynergyData synergyData = ScriptableObject.CreateInstance<SynergyData>();
                synergyData.name = $"SynergyData_{record.SynergyID}";
                
                typeof(SynergyData)
                    .GetField("synergyID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(synergyData, record.SynergyID);
                typeof(SynergyData)
                    .GetField("synergyName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(synergyData, record.SynergyName);
                typeof(SynergyData)
                    .GetField("synergyMainDescription", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(synergyData, record.SynergyMainDescription);
                typeof(SynergyData)
                    .GetField("synergyEffectScript1", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(synergyData, record.SynergyEffectScript1);
                typeof(SynergyData)
                    .GetField("synergyEffectScript2", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(synergyData, record.SynergyEffectScript2);
                typeof(SynergyData)
                    .GetField("synergyEffectScript3", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(synergyData, record.SynergyEffectScript3);
                typeof(SynergyData)
                    .GetField("synergyClassType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(synergyData, record.SynergyClassType);
                 typeof(SynergyData)
                    .GetField("heroIDCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(synergyData, record.HeroIDCount);
                 typeof(SynergyData)
                     .GetField("synergySkill1", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                     ?.SetValue(synergyData, record.SynergySkill1);
                 typeof(SynergyData)
                     .GetField("synergySkill2", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                     ?.SetValue(synergyData, record.SynergySkill2);
                
                string assetPath = $"Assets/Scriptables/SynergyDatas/SynergyData_{record.SynergyID}.asset";
                Directory.CreateDirectory("Assets/Scriptables/SynergyDatas");
                AssetDatabase.CreateAsset(synergyData, assetPath);

                manager.synergyList.Add(synergyData);
            }
        }
        
        EditorUtility.SetDirty(manager);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}