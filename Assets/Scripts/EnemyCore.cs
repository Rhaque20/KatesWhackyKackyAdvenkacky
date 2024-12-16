using System;
using System.Collections;
using UnityEngine;

public class EnemyCore : EntityCore
{
    protected KateCore _currentTarget = null;
    // Attacks the enemy can use against the player. Was intended to just be 2 hence the array over list.
    [SerializeField] protected EnemyAttack[] _enemyAttacks = new EnemyAttack[2];
    protected Coroutine _attackDelay = null;
    // Used for measurement purposes.
    protected float _capsuleRadius = 0f;

    // Used for in case the enemy is "spawned" as part of an enemy wave.
    protected bool _partOfWave = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    /// <summary>
    /// Called as the game starts to set up the necessary values.
    /// </summary>
    void Start()
    {
        base.Start();
        // Create an instance of the animator override as multiple instances of the enemy will
        // alter the animations inside the override which if they don't have their own instance of it
        // will cause all instances of the enemy to use the same animation changes.
        _animOverride = Instantiate(_animOverride);
        // Set the runtime controller to the animator override controller
        _anim.runtimeAnimatorController = _animOverride;
        // Prevents the core from ticking until it detects the player
        enabled = false;
        _capsuleRadius = GetComponent<CapsuleCollider2D>().size.x;
    }

    /// <summary>
    /// Called from the EnemyDetection script to allow the enemy to pursue the player
    /// </summary>
    /// <param name="kate"></param>
    public void AlertEnemy(KateCore kate)
    {
        Debug.Log("Alerted of kate! "+kate.name);
        _currentTarget = kate;
        // Set the movement target to the player now.
        GetComponent<EnemyMovement>().ChangeTarget(_currentTarget.transform);
        enabled = true;
    }

    /// <summary>
    /// Delay timer used to prevent the enemy from immediately attacking again.
    /// </summary>
    /// <param name="delay">Delay length</param>
    /// <returns></returns>
    protected IEnumerator AttackDelayTimer(float delay)
    {
        yield return new WaitForSeconds(delay);
        _attackDelay = null;
    }

    /// <summary>
    /// Called from Animation Event when attack is near end to let the enemy go, but also from StaggerComponent
    /// to ensure the enemy can be fully functioning again after recovering from a stagger.
    /// </summary>
    public override void Recover()
    {
        if (_attackDelay != null)
        {
            StopCoroutine(_attackDelay);
        }
        // Used in case current move is null to set the attack delay.
        float delay = _currentMove != null ? ((EnemyAttack)_currentMove).attackDelay : 1f;
        _attackDelay = StartCoroutine(AttackDelayTimer(delay));
        base.Recover();
    }

    /// <summary>
    /// Binds the enemy to the wave counter so when they die, they decrement the counter
    /// </summary>
    public void BindTargetToWaveCounter()
    {
        _partOfWave = true;
    }

    // Called from the health component albeit an event for ondeath.
    // If they are part of the wave counter, decrement their counter.
    public override void Death()
    {
        if (_partOfWave)
        {
            LevelManager.Instance.DecrementWaveCounter();
        }
    }

    /// <summary>
    /// Bulk of enemy AI. if player is near, they are not under cooldown and not already attacking, attack
    /// </summary>
    public void Update()
    {
        if(_currentTarget != null && _attackDelay == null)
        {
            // The use of the capsule measurements is to ensure the distance check takes the current radius into consideration.
            if (!_inAttack && (Mathf.Abs(_currentTarget.transform.position.x - transform.position.x) < 2f + _capsuleRadius))
            {
                _movement.disableMobility?.Invoke(false);
                _currentMove = _enemyAttacks[0];
                Attack();
            }
        }
        
    }
}
