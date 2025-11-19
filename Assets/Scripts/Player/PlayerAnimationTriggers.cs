using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationTriggers : MonoBehaviour
{
    private Player player => GetComponentInParent<Player>();

    public void AnimationTrigger() => player.AnimationTrigger();

    private void AttackTrigger()
    {
        AudioManager.instance.PlaySFX(2, null);

        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.attackCheck.position, player.attackCheckRadius);

        foreach (var hit in colliders)
        {
            if (hit.GetComponent<Enemy>() != null)
            {
                EnemyStats _target = hit.GetComponent<EnemyStats>();

                if(_target != null)
                    player.stats.DoDamage(_target);

                var weapon = Inventory.instance?.GetEquipment(EquipmentType.Weapon);
                weapon?.Effect(_target.transform);
            }
        }
    }
    
    private void ThrowSword()
    {
        SkillManager.instance.sword.CreateSword();
    }
}
