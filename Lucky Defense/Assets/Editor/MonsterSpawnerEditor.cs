using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(MonsterSpawner))]
    public class MonsterSpawnerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            MonsterSpawner spawner = (MonsterSpawner)target;
        
            int targetCount = (int)MonsterType.Count;
            while (spawner.monsterDataLists.Count < targetCount)
            {
                spawner.monsterDataLists.Add(new MonsterSpawner.MonsterDataList());
            }
            while (spawner.monsterDataLists.Count > targetCount)
            {
                spawner.monsterDataLists.RemoveAt(spawner.monsterDataLists.Count - 1);
            }

            for (int i = 0; i < spawner.monsterDataLists.Count; i++)
            {
                EditorGUILayout.LabelField(((MonsterType)i).ToString());
                SerializedProperty listProperty = serializedObject.FindProperty($"monsterDataLists.Array.data[{i}].dataList");
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
            if (string.IsNullOrEmpty(path)) return;
        
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
                    Debug.Assert(record.Type >= 0 && record.Type < (int)MonsterType.Count, $"Invalid type value for Monster ID {record.Id}");

                    MonsterType type = (MonsterType)record.Type;

                    MonsterData monsterData = ScriptableObject.CreateInstance<MonsterData>();
                    monsterData.name = $"Monster_{record.Id}";

                    typeof(MonsterData).GetField("id", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(monsterData, record.Id);
                    typeof(MonsterData).GetField("type", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(monsterData, type);
                    typeof(MonsterData).GetField("hp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(monsterData, record.Hp);
                    typeof(MonsterData).GetField("speed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(monsterData, record.Speed);
                    typeof(MonsterData).GetField("rewardType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(monsterData, record.RewardType);
                    typeof(MonsterData).GetField("quantity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(monsterData, record.Quantity);

                    string assetPath = $"Assets/Scriptables/MonsterDatas/Monster_{record.Id}.asset";
                    Directory.CreateDirectory("Assets/Scriptables/MonsterDatas");
                    AssetDatabase.CreateAsset(monsterData, assetPath);

                    spawner.monsterDataLists[(int)type].dataList.Add(monsterData);
                }
            }
        
            EditorUtility.SetDirty(spawner);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
