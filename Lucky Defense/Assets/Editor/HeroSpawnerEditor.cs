using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HeroSpawner))]
public class HeroSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        HeroSpawner spawner = (HeroSpawner)target;

        if (GUILayout.Button("Load HeroSummonProbabilityData CSV"))
        {
            LoadCsvAndCreateHeroSummonProbabilityDataScriptables(spawner);
        }

        int targetCount = Utility.HeroGradeCount;
        while (spawner.heroDataRarityLists.Count < targetCount)
        {
            spawner.heroDataRarityLists.Add(new HeroSpawner.HeroDataList());
        }

        while (spawner.heroDataRarityLists.Count > targetCount)
        {
            spawner.heroDataRarityLists.RemoveAt(spawner.heroDataRarityLists.Count - 1);
        }

        for (int i = 0; i < spawner.heroDataRarityLists.Count; ++i)
        {
            EditorGUILayout.LabelField(((HeroGrade)i+1).ToString());
            SerializedProperty listProperty =
                serializedObject.FindProperty($"heroDataRarityLists.Array.data[{i}].dataList");
            EditorGUILayout.PropertyField(listProperty, true);
        }

        if (GUILayout.Button("Load HeroData CSV"))
        {
            LoadCsvAndCreateHeroDataScriptables(spawner);
        }

        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(spawner);
        }
    }

    private void LoadCsvAndCreateHeroDataScriptables(HeroSpawner spawner)
    {
        string path = EditorUtility.OpenFilePanel("Load Hero Data CSV", "Assets/Resources/Tables", "csv");
        if (string.IsNullOrEmpty(path)) return;

        foreach (var list in spawner.heroDataRarityLists)
        {
            list.dataList.Clear();
        }

        using (var reader = new StreamReader(path))
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            var records = csv.GetRecords<HeroCsvData>();

            foreach (var record in records)
            {
                Debug.Assert(1 <= record.Grade && record.Grade <= Utility.HeroGradeCount,
                    $"Invalid rarity value for Hero ID {record.HeroID}");

                HeroGrade grade = (HeroGrade)record.Grade;

                HeroData heroData = ScriptableObject.CreateInstance<HeroData>();
                heroData.name = $"Hero_{record.HeroID}";

                typeof(HeroData)
                    .GetField("heroID",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(heroData, record.HeroID);
                typeof(HeroData)
                    .GetField("stringKey",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(heroData, record.StringKey);
                typeof(HeroData)
                    .GetField("grade",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(heroData, grade);
                typeof(HeroData)
                    .GetField("atkType",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(heroData, record.AtkType);
                typeof(HeroData)
                    .GetField("atkRange",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(heroData, record.AtkRange);
                typeof(HeroData)
                    .GetField("heroDamage",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(heroData, record.HeroDamage);
                typeof(HeroData)
                    .GetField("atkSpeed",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(heroData, record.AtkSpeed);
                typeof(HeroData)
                    .GetField("heroSkill",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(heroData, record.HeroSkill);
                typeof(HeroData)
                    .GetField("blockCost",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(heroData, record.BlockCost);
                typeof(HeroData)
                    .GetField("saleType",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(heroData, record.SaleType);
                typeof(HeroData)
                    .GetField("saleQuantity",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(heroData, record.SaleQuantity);
                

                string assetPath = $"Assets/Scriptables/HeroDatas/Hero_{record.HeroID}.asset";
                Directory.CreateDirectory("Assets/Scriptables/HeroDatas");
                AssetDatabase.CreateAsset(heroData, assetPath);

                spawner.heroDataRarityLists[(int)grade-1].dataList.Add(heroData);
            }
        }

        EditorUtility.SetDirty(spawner);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void LoadCsvAndCreateHeroSummonProbabilityDataScriptables(HeroSpawner spawner)
    {
        string path =
            EditorUtility.OpenFilePanel("Load HeroSummonProbability Data CSV", "Assets/Resources/Tables", "csv");
        if (string.IsNullOrEmpty(path)) return;

        spawner.heroSummonProbabilityDataLists.Clear();

        using (var reader = new StreamReader(path))
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            var records = csv.GetRecords<HeroSummonProbabilityCsvData>();

            foreach (var record in records)
            {
                HeroSummonProbabilityData heroSummonProbabilityData =
                    ScriptableObject.CreateInstance<HeroSummonProbabilityData>();
                heroSummonProbabilityData.name = $"HeroSummonProbabilityData_{record.Id}";

                typeof(HeroSummonProbabilityData)
                    .GetField("id", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(heroSummonProbabilityData, record.Id);
                typeof(HeroSummonProbabilityData)
                    .GetField("commonProbability",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(heroSummonProbabilityData, record.CommonProbability);
                typeof(HeroSummonProbabilityData)
                    .GetField("rareProbability",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(heroSummonProbabilityData, record.RareProbability);
                typeof(HeroSummonProbabilityData)
                    .GetField("heroicProbability",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(heroSummonProbabilityData, record.HeroicProbability);
                typeof(HeroSummonProbabilityData)
                    .GetField("legendaryProbability",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(heroSummonProbabilityData, record.LegendaryProbability);

                string assetPath =
                    $"Assets/Scriptables/HeroSummonProbabilityDatas/HeroSummonProbabilityData_{record.Id}.asset";
                Directory.CreateDirectory("Assets/Scriptables/HeroSummonProbabilityDatas");
                AssetDatabase.CreateAsset(heroSummonProbabilityData, assetPath);

                spawner.heroSummonProbabilityDataLists.Add(heroSummonProbabilityData);
            }
        }

        EditorUtility.SetDirty(spawner);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}