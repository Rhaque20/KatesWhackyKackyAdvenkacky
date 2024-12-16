using System;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    // Default value only matters if attached to an object without an Entity Core.
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] float _currentHealth = 100f;
    [SerializeField] private Image _healthBarFill;
    // Used to play the hurt sound effect when taking damage.
    SoundManager _soundManager;
    private EntityCore _entityData;
    // Called when HP hits 0.
    private Action _onDeath;
    // Called when target is hit, though went unused.
    private Action _onHit;

    /// <summary>
    /// Getter/Setter for _onDeath
    /// </summary>
    public Action onDeath
    {
        get { return _onDeath; }
        set { _onDeath += value; }
    }
    /// <summary>
    /// Getter/Setter for onHit;
    /// </summary>
    public Action onHit
    {
        get { return _onHit; }
        set { _onHit += value; }
    }

    /// <summary>
    /// Gets health ratio
    /// </summary>
    public float healthRatio
    {
        get { return _currentHealth / _maxHealth; }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    /// <summary>
    /// Initialize the necessary values
    /// </summary>
    protected void Start()
    {
        // Set max HP based on the data in the entity core.
        _entityData = GetComponent<EntityCore>();
        if(_entityData != null)
        {
            _maxHealth = _entityData.ReturnStatValue(EntityCore.Stats.Health);
        }

        _currentHealth = _maxHealth;
        // Get the sound manager to play the hurt sound.
        _soundManager = GetComponent<SoundManager>();
    }

    /// <summary>
    /// Called when HP hits 0. Invoke the action for all functions subscribed to it
    /// before setting this gameObject inactive.
    /// </summary>
    public virtual void Death()
    {
        onDeath?.Invoke();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Called frequently for when receiving damage either from a projectile, melee attack or damage plane.
    /// </summary>
    /// <param name="damage">Damage received</param>
    /// <param name="isMagic">Is the damage magic type or not.</param>
    public virtual void ReceiveDamage(float damage, bool isMagic)
    {
        // if the damage is greater than 0, play the hit sound.
        if(damage > 0 && _soundManager != null)
        {
            _soundManager.PlayHit();
        }

        // Deduct current health based on the damage received.
        _currentHealth -= damage;

        // Adjust the bar fill amount based on the current ratio;
        if (_healthBarFill != null)
        {
            _healthBarFill.fillAmount = _currentHealth/_maxHealth;
        }

        // If it falls below 0, call Death Function.
        if (_currentHealth <= 0f)
        {
            Death();
        }

    }

    /// <summary>
    /// Restores health based on amount healed.
    /// Only ever called when player eats a turkey leg for heal burst.
    /// </summary>
    /// <param name="healAmount">Amount restored</param>
    public void Heal(float healAmount)
    {
        // Edge case for when healing amount is a value between 0.0 and 1.0
        // which means to do a % scale of max health rather than flat amount.
        if(healAmount < 1f)
        {
            if (healAmount > 0f)
            {
                _currentHealth += _maxHealth * healAmount;
            }
            else // in case healing is actually 0
                return;
        }
        else// Else restore based on amount given.
        {
            _currentHealth += healAmount;
        }

        // Clamp health to avoid it overflowing.
        _currentHealth = Mathf.Clamp(_currentHealth, 0f, _maxHealth);

        // Update bar to new ratio
        if (_healthBarFill != null)
        {
            _healthBarFill.fillAmount = _currentHealth / _maxHealth;
        }
    }
}
