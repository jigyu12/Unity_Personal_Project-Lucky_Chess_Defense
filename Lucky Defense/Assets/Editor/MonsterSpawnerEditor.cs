using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonsterSpawner))]
public class MonsterSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MonsterSpawner spawner = (MonsterSpawner)target;

        int targetCount = Utility.MonsterTypeCount;
        while (spawner.monsterDataLists.Count < targetCount)
        {
            spawner.monsterDataLists.Add(new MonsterSpawner.MonsterDataList());
        }

        while (spawner.monsterDataLists.Count > targetCount)
        {
            spawner.monsterDataLists.RemoveAt(spawner.monsterDataLists.Count - 1);
        }

        for (int i = 0; i < spawner.monsterDataLists.Count; ++i)
        {
            EditorGUILayout.LabelField(((MonsterType)i+1).ToString());
            SerializedProperty listProperty =
                serializedObject.FindProperty($"monsterDataLists.Array.data[{i}].dataList");
            EditorGUILayout.PropertyField(listProperty, true);
        }

        if (GUILayout.Button("Load MonsterData CSV"))
        {
            LoadCsvAndCreateMonsterDataScriptables(spawner);
        }

        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(spawner);
        }
    }

    private void LoadCsvAndCreateMonsterDataScriptables(MonsterSpawner spawner)
    {
        string path = EditorUtility.OpenFilePanel("Load Monster Data CSV", "Assets/Resources/Tables", "csv");
        if (string.IsNullOrEmpty(path)) 
            return;

        foreach (var list in spawner.monsterDataLists)
        {
            list.dataList.Clear();
        }

        using (var reader = new StreamReader(path))
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            var records = csv.GetRecords<MonsterCsvData>();

            foreach (var record in records)
            {
                Debug.Assert(record.MonType >= 1 && record.MonType <= Utility.MonsterTypeCount,
                    $"Invalid type value for Monster ID {record.MonsterID}");

                MonsterType type = (MonsterType)record.MonType;

                MonsterData monsterData = ScriptableObject.CreateInstance<MonsterData>();
                monsterData.name = $"Monster_{record.MonsterID}";

                typeof(MonsterData)
                    .GetField("monsterID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(monsterData, record.MonsterID);
                typeof(MonsterData)
                    .GetField("stringKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(monsterData, record.StringKey);
                typeof(MonsterData)
                    .GetField("monType",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(monsterData, type);
                typeof(MonsterData)
                    .GetField("monHp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(monsterData, record.MonHp);
                typeof(MonsterData)
                    .GetField("monSpeed",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(monsterData, record.MonSpeed);
                typeof(MonsterData)
                    .GetField("monReward",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(monsterData, record.MonReward);
                typeof(MonsterData)
                    .GetField("monQuantity",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(monsterData, record.MonQuantity);

                string assetPath = $"Assets/Scriptables/MonsterDatas/Monster_{record.MonsterID}.asset";
                Directory.CreateDirectory("Assets/Scriptables/MonsterDatas");
                AssetDatabase.CreateAsset(monsterData, assetPath);

                spawner.monsterDataLists[(int)type - 1].dataList.Add(monsterData);
            }
        }

        EditorUtility.SetDirty(spawner);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}