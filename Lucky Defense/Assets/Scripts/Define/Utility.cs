using System;

public static class Utility
{
    public static int HeroGradeCount => Enum.GetValues(typeof(HeroGrade)).Length;
    public static int MonsterTypeCount => Enum.GetValues(typeof(MonsterType)).Length;

    public static readonly String HeroPrefabPath = "Assets/Resources/Prefabs/Spum/Hero/";
    public static readonly String MonsterPrefabPath = "Assets/Resources/Prefabs/Spum/Monster/";
}