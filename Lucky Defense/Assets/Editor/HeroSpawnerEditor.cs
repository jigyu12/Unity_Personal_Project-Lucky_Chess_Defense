using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(HeroSpawner))]
    public class HeroSpawnerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            HeroSpawner spawner = (HeroSpawner)target;
        
            if (GUILayout.Button("Load HeroSummonProbabilityData CSV"))
            {
                LoadCsvAndCreateHeroSummonProbabilityDataScriptables(spawner);
            }

            int targetCount = (int)HeroRarity.Count;
            while (spawner.heroDataRarityLists.Count < targetCount)
            {
                spawner.heroDataRarityLists.Add(new HeroSpawner.HeroDataList());
            }
            while (spawner.heroDataRarityLists.Count > targetCount)
            {
                spawner.heroDataRarityLists.RemoveAt(spawner.heroDataRarityLists.Count - 1);
            }

            for (int i = 0; i < spawner.heroDataRarityLists.Count; i++)
            {
                EditorGUILayout.LabelField(((HeroRarity)i).ToString());
                SerializedProperty listProperty = serializedObject.FindProperty($"heroDataRarityLists.Array.data[{i}].dataList");
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
                    Debug.Assert(record.Rarity >= 0 && record.Rarity < (int)HeroRarity.Count, $"Invalid rarity value for Hero ID {record.HeroId}");

                    HeroRarity rarity = (HeroRarity)record.Rarity;

                    HeroData heroData = ScriptableObject.CreateInstance<HeroData>();
                    heroData.name = $"Hero_{record.HeroId}";

                    typeof(HeroData).GetField("heroId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(heroData, record.HeroId);
                    typeof(HeroData).GetField("attackRange", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(heroData, record.AttackRange);
                    typeof(HeroData).GetField("damage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(heroData, record.Damage);
                    typeof(HeroData).GetField("rarity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(heroData, rarity);
                    typeof(HeroData).GetField("cost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(heroData, record.Cost);
                    typeof(HeroData).GetField("attackMethod", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(heroData, record.AttackMethod);
                    typeof(HeroData).GetField("attackSpeed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(heroData, record.AttackSpeed);

                    string assetPath = $"Assets/Scriptables/HeroDatas/Hero_{record.HeroId}.asset";
                    Directory.CreateDirectory("Assets/Scriptables/HeroDatas");
                    AssetDatabase.CreateAsset(heroData, assetPath);

                    spawner.heroDataRarityLists[(int)rarity].dataList.Add(heroData);
                }
            }

            EditorUtility.SetDirty(spawner);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void LoadCsvAndCreateHeroSummonProbabilityDataScriptables(HeroSpawner spawner)
        {
            string path = EditorUtility.OpenFilePanel("Load HeroSummonProbability Data CSV", "Assets/Resources/Tables", "csv");
            if (string.IsNullOrEmpty(path)) return;

            spawner.heroSummonProbabilityDataLists.Clear();

            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                var records = csv.GetRecords<HeroSummonProbabilityCsvData>();

                foreach (var record in records)
                {
                    HeroSummonProbabilityData heroSummonProbabilityData = ScriptableObject.CreateInstance<HeroSummonProbabilityData>();
                    heroSummonProbabilityData.name = $"HeroSummonProbabilityData_{record.Id}";

                    typeof(HeroSummonProbabilityData).GetField("id", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(heroSummonProbabilityData, record.Id);
                    typeof(HeroSummonProbabilityData).GetField("commonProbability", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(heroSummonProbabilityData, record.CommonProbability);
                    typeof(HeroSummonProbabilityData).GetField("rareProbability", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(heroSummonProbabilityData, record.RareProbability);
                    typeof(HeroSummonProbabilityData).GetField("heroicProbability", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(heroSummonProbabilityData, record.HeroicProbability);
                    typeof(HeroSummonProbabilityData).GetField("legendaryProbability", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(heroSummonProbabilityData, record.LegendaryProbability);

                    string assetPath = $"Assets/Scriptables/HeroSummonProbabilityDatas/HeroSummonProbabilityData_{record.Id}.asset";
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
}