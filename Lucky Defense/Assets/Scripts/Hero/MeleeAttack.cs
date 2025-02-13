using UnityEngine.Events;

public class MeleeAttack : IAttackMethod
{
    public void Attack(Monster monster, int damage, UnityAction<Monster> attackSkillToMon)
    { 
        attackSkillToMon?.Invoke(monster);
        monster.OnDamaged(damage);
    }
}