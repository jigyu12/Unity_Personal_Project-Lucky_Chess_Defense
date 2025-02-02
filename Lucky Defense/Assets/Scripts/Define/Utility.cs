using System;

public static class Utility
{
    public static int HeroGradeCount => Enum.GetValues(typeof(HeroGrade)).Length;
    public static int HeroAttackTypeCount => Enum.GetValues(typeof(HeroAttackType)).Length;
    public static int MonsterTypeCount => Enum.GetValues(typeof(MonsterType)).Length;
}