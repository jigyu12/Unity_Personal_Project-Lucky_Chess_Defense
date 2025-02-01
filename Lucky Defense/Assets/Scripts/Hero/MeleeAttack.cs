public class MeleeAttack : IAttackMethod
{
    public void Attack(Monster monster, int damage)
    {
        monster.OnDamaged(damage);
    }
}