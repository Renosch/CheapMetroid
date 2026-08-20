using UnityEngine;

using Weapon;

public interface IDamageable
{
    public void TakeDamage(BeamScript data);
    public void TakeBombDamage(BombScript data);
    public void TakeDamageAndFreeze(BeamScript data);
}
