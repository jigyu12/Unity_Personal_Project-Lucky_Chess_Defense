using System;

public static class Utility
{
    public static int HeroGradeCount => Enum.GetValues(typeof(HeroGrade)).Length;
    public static int MonsterTypeCount => Enum.GetValues(typeof(MonsterType)).Length;
    public static int SynergyClassCount => Enum.GetValues(typeof(SynergyClass)).Length;

    public static readonly string HeroPrefabPath = "Assets/DownloadPackages/Prefabs/Spum/Hero/";
    public static readonly string MonsterPrefabPath = "Assets/DownloadPackages/Prefabs/Spum/Monster/";
}