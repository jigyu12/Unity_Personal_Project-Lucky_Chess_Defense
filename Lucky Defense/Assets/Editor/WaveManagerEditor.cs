using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WaveManager))]
public class WaveManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WaveManager manager = (WaveManager)target;

        if (GUILayout.Button("Load WaveData CSV"))
        {
            LoadCsvAndCreateWaveDataScriptables(manager);
        }

        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
        }
    }

    private void LoadCsvAndCreateWaveDataScriptables(WaveManager manager)
    {
        string path = EditorUtility.OpenFilePanel("Load Wave Data CSV", "Assets/Resources/Tables", "csv");
        if (string.IsNullOrEmpty(path)) return;

        manager.waveDataList.Clear();

        using (var reader = new StreamReader(path))
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            var records = csv.GetRecords<WaveCsvData>();

            foreach (var record in records)
            {
                WaveData waveData = ScriptableObject.CreateInstance<WaveData>();
                waveData.name = $"WaveData_{record.WaveID}";

                typeof(WaveData)
                    .GetField("waveID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(waveData, record.WaveID);
                typeof(WaveData)
                    .GetField("waveNumber",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(waveData, record.WaveNumber);
                typeof(WaveData)
                    .GetField("monsterID",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(waveData, record.MonsterID);
                typeof(WaveData)
                    .GetField("monHpMlt",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(waveData, record.MonHpMlt);
                typeof(WaveData)
                    .GetField("createMonNumber",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(waveData, record.CreateMonNumber);
                typeof(WaveData)
                    .GetField("waveTime",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(waveData, record.WaveTime);

                string assetPath = $"Assets/Scriptables/WaveDatas/WaveData_{record.WaveID}.asset";
                Directory.CreateDirectory("Assets/Scriptables/WaveDatas");
                AssetDatabase.CreateAsset(waveData, assetPath);

                manager.waveDataList.Add(waveData);
            }
        }

        EditorUtility.SetDirty(manager);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}