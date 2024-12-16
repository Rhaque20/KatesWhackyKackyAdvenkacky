using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAttack", menuName = "ScriptableObjects/EnemyAttackData", order = 1)]
public class EnemyAttack : AttackData
{
    // This is the most deviation there is. Determines the cooldown until the next attack.
    [SerializeField] private float _attackDelay = 2f;

    public float attackDelay
    {
        get { return _attackDelay; }
    }
}
