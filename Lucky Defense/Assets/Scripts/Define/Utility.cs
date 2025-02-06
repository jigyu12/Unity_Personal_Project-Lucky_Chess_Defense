using System;

public static class Utility
{
    public static int HeroGradeCount => Enum.GetValues(typeof(HeroGrade)).Length;
    public static int MonsterTypeCount => Enum.GetValues(typeof(MonsterType)).Length;

    public static String HeroPrefabPath = "Prefabs/Spum/Hero/";
    public static String MonsterPrefabPath = "Prefabs/Spum/Monster/";
}