using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EntityCore : MonoBehaviour
{
    // The base class to be used for all future logic
    public enum Stats { Health, Attack, Magic, Defense,Spirit};
    // Using custom named array attribute, can name the indices on the editor UI to something else
    // sadly only health and attack are even used in the end.
    [SerializeField, NamedArrayAttribute(new string[] { "Health", "Attack", "Magic", "Defense", "Spirit" })] 
    protected float[] _stats = new float[5];
    // Current move that is being used.
    protected AttackData _currentMove = null;
    // Used to determine if actively in attack, though the _inAttackState went unused.
    protected bool _inAttack = false, _inAttackState = false;
    protected Animator _anim;
    protected BoxCollider2D _hitBox;
    protected Movement _movement;
    protected CapsuleCollider2D _capsuleCollider;
    protected Rigidbody2D _rigidBody;

    protected Vector3 _spriteScale;

    [SerializeField] protected AnimatorOverrideController _animOverride;
    [SerializeField] protected LayerMask _hitMask;
    protected bool _canDealContactDamage = false;
    
    /// <summary>
    /// Initialize all the necessary variables by the components.
    /// </summary>
    public void Start()
    {
        _anim = transform.GetChild(0).GetComponent<Animator>();
        _hitBox = _anim.transform.GetChild(0).GetComponent<BoxCollider2D>();
        _capsuleCollider = GetComponent<CapsuleCollider2D>();
        _rigidBody = GetComponent<Rigidbody2D>();
        _movement = GetComponent<Movement>();
        // Bind the Death function from EntityCore to the onDeath delegate in health component
        GetComponent<Health>().onDeath += Death;

        // Use the original sprite scale to scale the boxcast based on the scale of the sprite
        // as the hurtbox is the child to the sprite which is the child of the gameobject with this
        // component.
        _spriteScale = transform.GetChild(0).localScale;
    }

    /// <summary>
    /// Propels the entity based on the selfPropelForce of the current move.
    /// </summary>
    public virtual void LaunchSelf()
    {
        if (_currentMove != null)
        {
            Debug.Log("Launching self with current move "+_currentMove.name);
            _movement.Launch(_currentMove.selfPropelForces);
        }
        else
        {
            Debug.Log("Current move is null");
        }
    }

    /// <summary>
    /// Sets the contact damage bool. Designed like flip flop so simply calling it
    /// will enable or disable it based on order.
    /// </summary>
    public virtual void TriggerContactDamage()
    {
        _canDealContactDamage = !_canDealContactDamage;
    }

    /// <summary>
    /// Unused, was meant to allow the player or enemy to enter a state where they can only keep attacking.
    /// </summary>
    public virtual void EnterAttackState()
    {
        _movement.disableMobility?.Invoke(_inAttackState);
        _inAttackState = !_inAttackState;
        _anim.SetBool("AttackState", _inAttackState);
    }

    /// <summary>
    /// Called when player dies which is done by the onDeath delegate on Health.
    /// </summary>
    public virtual void Death()
    {

    }

    /// <summary>
    /// Used to gather the final stat values after all modifiers were applied.
    /// </summary>
    /// <param name="statType">Stat that is being processed.</param>
    /// <returns>Final stat value after all modifiers</returns>
    public float ReturnStatValue(Stats statType)
    {
        switch(statType)
        {
            case Stats.Health:
                return _stats[(int)Stats.Health];
            case Stats.Attack:
                return _stats[(int)Stats.Attack];
            case Stats.Magic:
                return _stats[(int)Stats.Magic];
            case Stats.Defense:
                return _stats[(int)Stats.Defense];
            case Stats.Spirit:
                return _stats[(int)Stats.Spirit];
        }
        return 0.0f;
    }

    /// <summary>
    /// Determines if the player recovers from the interrupt
    /// </summary>
    /// <param name="recoverFromInterrupt">The boolean to determine if interrupt occurs or not</param>
    public virtual void InterruptAction(bool recoverFromInterrupt)
    {
        if(!recoverFromInterrupt)
        {
            _anim.Play("Stagger");
            _inAttack = true;
        }
        else
        {
            _anim.SetTrigger("BreakFree");
            Recover();
        }
        
    }

    /// <summary>
    /// Shared logic for all instances, sets up the necessary animations in the animator override controller
    /// before jumping straight to the state to call it. As the animator override controller can freely replace animations
    /// and its using sprite based animations, no side effects can occur.
    /// </summary>
    public virtual void Attack()
    {

        if(_currentMove != null)
        {
            Debug.Log("Activating "+_currentMove.name);
            _inAttack = true;
            _movement.disableMobility?.Invoke(false);
            _animOverride["Attack"] = _currentMove.animations[0];
            _animOverride["Recover"] = _currentMove.animations[1];
            _anim.Play("Attack");
            // In case the trigger was active but never consumed leaving it lingering
            // this occurs when the enemy has been staggered forcing them into a completely different state.
            _anim.ResetTrigger("BreakFree");
        }
    }

    /// <summary>
    /// Base recovery that re-enables movement and indicates of no longer in attack state.
    /// </summary>
    public virtual void Recover()
    {
        _inAttack = false;
        _movement.disableMobility?.Invoke(true);
    }

    /// <summary>
    /// To be used in child classes.
    /// </summary>
    public virtual void SpecialAction()
    {

    }

    /// <summary>
    /// Fires a projectile based on the velocity from the skill 
    /// and the force from the animation event data.
    /// Should've been in parent, but only player actually uses it, so it's left blank.
    /// </summary>
    /// <param name="force"></param>
    public virtual void FireProjectile(float force)
    {

    }

    /// <summary>
    /// The final function call that will handle hit logic if a player or enemy ends up here.
    /// Primarily stagger and lowering health.
    /// </summary>
    /// <param name="target"></param>
    public virtual void Hit(Collider2D target)
    {
        StaggerComponent _staggerComp;
        Health healthComp;
        // If the target has a health component, do the following.
        if(target.TryGetComponent<Health>(out healthComp))
        {
            Debug.Log("Hitting " + target.name);
            // Damage is scaled off of modifier of the attack data and the attack stat established before.
            healthComp.ReceiveDamage(_stats[(int)Stats.Attack] * _currentMove.modifier, false);
            // Only called on destructible objects that don't have stagger components
            // which don't exist.
            if (target.TryGetComponent<StaggerComponent>(out _staggerComp))
            {
                _staggerComp.ReceiveHit();
            }
        }
    }
    /// <summary>
    /// Variant of Hit Function to be called from animation events since they do not support many types
    /// only int, float, and/or string
    /// </summary>
    public virtual void Hit()
    {
        
        List<Collider2D> hitResults = new List<Collider2D>();
        // Enable the hitbox for a frame just enough to scan the targets inside it.
        _hitBox.gameObject.SetActive(true);
        // Get the hit results based on who was inside the box cast.
        RaycastHit2D[]results =  Physics2D.BoxCastAll(_hitBox.transform.position, _hitBox.transform.localScale * _spriteScale.y, 0f, Vector2.zero, 0f, _hitMask);
        //_hitBox.Overlap(_contactFilter,hitResults);
        _hitBox.gameObject.SetActive(false);

        StaggerComponent _staggerComp;
        Health healthComp;

        if (results.Length > 0)
        {
            foreach(RaycastHit2D col in results)
            {
                // Pass all the hit results to the final Hit function.
                Debug.Log("Struck "+col.collider.name);
                Hit(col.collider);
            }
        }
    }
}
