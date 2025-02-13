using UnityEngine.Events;

public interface IAttackMethod
{
    void Attack(Monster monster, int damage, UnityAction<Monster> attackSkillToMon);
}