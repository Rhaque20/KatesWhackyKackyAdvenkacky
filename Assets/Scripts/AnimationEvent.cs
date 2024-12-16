using UnityEngine;

public class AnimationEvent : MonoBehaviour
{
    Animator _anim;
    EntityCore _entity;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    /// <summary>
    /// Called when the game starts. Gathers the required components from the parent 
    /// as the animations occur at the child object rather than the parent where most of the components are.
    /// This separation is to ensure the sprites can be whatever size I want without it influencing the hitbox.
    /// </summary>
    void Start()
    {
        _anim = GetComponent<Animator>();
        _entity = transform.GetComponentInParent<EntityCore>();
    }

    /// <summary>
    /// Called when the animation event is called to scan hits.
    /// </summary>
    public virtual void Hit()
    {
        if (_entity != null)
        {
            _entity.Hit();
        }
    }

    /// <summary>
    /// Called on the animation event to fire a projectile. The velocity is determined by the attack data
    /// so the propulsion power on said velocity is dictated here.
    /// </summary>
    /// <param name="force"></param>
    public virtual void FireProjectile(float force)
    {
        if (_entity != null)
        {
            _entity.FireProjectile(force);
        }
    }

    /// <summary>
    /// Nebulous intentionally to allow unique actions tied to their core to be done here while making it
    /// Usable by any children of EntityCore.
    /// </summary>
    public virtual void SpecialAction()
    {
        if(_entity != null)
        {
            _entity.SpecialAction();
        }
    }

    /// <summary>
    /// Use to enable movement for the player and enemy when finishing the attack
    /// but also used to chain attacks before the animation ends.
    /// </summary>
    public virtual void Recover()
    {
        if (_entity != null)
        {
            _entity.Recover();
        }
    }

    /// <summary>
    /// Used for the player namely and less on the enemy to propel them forward when swinging.
    /// </summary>
    public virtual void LaunchSelf()
    {
        if (_entity != null)
        {
            _entity.LaunchSelf();
        }
    }

    /// <summary>
    /// Also meant to be used with enemies but just used on player to initiate contact damage
    /// for the Dash Jump X Z (DJXZ)
    /// </summary>
    public virtual void StartContactDamage()
    {
        if(_entity != null)
        {
            _entity.TriggerContactDamage();
        }
    }

    
}
