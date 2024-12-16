using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Linq;
using System;
using System.Collections;
using static Unity.Cinemachine.CinemachineFreeLookModifier;

public class KateCore : EntityCore
{
    const int PLAYERLAYER = 7, ENEMYLAYER = 8;
    const char UP = '^', DOWN = 'V', LEFT = '<', RIGHT = '>', LIGHT = 'Z', HEAVY = 'X';
    const char DASH = 'D', JUMP = 'J';
    // The array of all possible combos the player has. Done for editor purposes since I would need to make
    // a custom serialize for dictionaries so I can drop the data into those without needing an array
    [SerializeField]KateAttackData[] _attackData;
    // Used to keep track of the specific ammo stored in each index (kinda like a revolver's rotating ammo carrier)
    [SerializeField] private int[] _ammoLoaded;
    // Used to do the special effect indicating ammo type obtained.
    [SerializeField] private GameObject _elementalWheelPrefab;
    // 6 is arbitrary but is the number of cannon balls chung used in Elsword
    [SerializeField] int _maxAmmo = 6;
    // How far the player will travel after doing Dash X.
    [SerializeField] private float _cannonDashDistance = 2f;

    // The final container of the combo types. Each combo is stored in a string and extracted when the correct
    // combo is inputted.
    Dictionary<string, KateAttackData> _attackMap = new Dictionary<string, KateAttackData>();
    private string _currentComboString = "";
    // Matters for combo purposes.
    bool _attackBuffer = false, _isSprinting = false, _inAir = false;
    // Also matters for combo purposes like Down + X
    Vector2 _directionalInput = Vector2.zero;
    private PlayerMovement _playerMovement;
    // Current kate attack data. takes the original attack data from the parent and casts its child.
    private KateAttackData _currentKateAttack;
    // Player health but also used to keep track of their cannon balls and turkey legs.
    private PlayerStatus _playerStatus;
    
    // Current ammo and turkey capacity.
    int _ammo = 0, _turkeyCapacity = 0;
    // Used for ammo tracker purposes
    AttackData.Element _lastUsedAmmoType = AttackData.Element.Neutral;
    // Transform ref of the prefab.
    private Transform _elementalWheelRef;
    // Used to offset the position of the elemental wheel based on capsule height.
    private float _capsuleOffset = 0.0f;

    private Coroutine _dropComboTimer;

    /// <summary>
    ///  Sets up the necessary components and adds all the 
    ///  attack data from the array to the dictionary.
    /// </summary>
    public void Start()
    {
        base.Start();

        _ammoLoaded = new int[_maxAmmo];
        Array.Fill(_ammoLoaded, -1);

        foreach(KateAttackData _data in _attackData)
        {
            _attackMap.Add(_data.requiredComboString, _data);
        }

        _playerStatus = GetComponent<PlayerStatus>();

        if (_elementalWheelPrefab != null)
        {
            _elementalWheelRef = Instantiate(_elementalWheelPrefab).transform;
        }
        else
        {
            Debug.LogError("Please initialize elemental wheel with the prefab of the same name");
        }
        _capsuleOffset = GetComponent<CapsuleCollider2D>().offset.y;
        _playerMovement = (PlayerMovement)_movement;
        // Used to keep track of if the player inputted a sprint or a jump for the
        // Dash and/or jump combos.
        _playerMovement.SetUpSprintFunction(StartedSprint);
        _playerMovement.SetUpJumpFunction(StartedJump);
        
    }

    private IEnumerator DropComboTimer()
    {
        yield return new WaitForSeconds(2f);
        _dropComboTimer = null;
        _currentComboString = string.Empty;
    }

    /// <summary>
    /// Used for combo purposes to register the dash input
    /// </summary>
    /// <param name="sprinting">Is actively sprinting</param>
    public void StartedSprint(bool sprinting)
    {
        Debug.Log("Calling core start sprinting"+sprinting);
        _isSprinting = sprinting;
    }

    /// <summary>
    /// Used for combo purposes to register the jump input.
    /// </summary>
    /// <param name="inAir">Did they initiate a jump.</param>
    public void StartedJump(bool inAir)
    {
        Debug.Log("Starting jump");
        _inAir = inAir;
        if(inAir)
        {
            // Checks if they are also in the middle of dashing
            // So the dash can be pre-pended on the string and then the Jump.
            char? directional = DirectionalInput();
            if (directional != null && directional == DASH)
            {
                _currentComboString += DASH;
            }
            _currentComboString += JUMP;
        }
        else
        {
            // Else empty out the string to prevent potential softlocks.
            _currentComboString = string.Empty;
            base.Recover();
        }
    }

    /// <summary>
    ///  Used when colliding with a turkey and determine if the player has the capacity
    ///  to hold the turkey.
    /// </summary>
    /// <returns></returns>
    public bool CollectTurkey()
    {
        // Turkey capacity is dictated by ammo capacity for consistency.
        if (_turkeyCapacity > _maxAmmo)
            return false;

        _turkeyCapacity++;
        // Adds turkey to the turkey counter on Player Status.
        _playerStatus.AddTurkey(_turkeyCapacity);

        return true;
    }

    /// <summary>
    /// Input Action to let the player eat the turkey to heal.
    /// </summary>
    /// <param name="ctx">Input action value</param>
    public void EatTurkey(InputAction.CallbackContext ctx)
    {
        if (_playerStatus.healthRatio >= 1f)
            return;

        _playerStatus.Heal(0.4f);// Heal based on 40% max HP.
        _turkeyCapacity--;// Reduce turkey count
        _playerStatus.RemoveTurkey(_turkeyCapacity);// Remove a turkey from the visual counter.
    }


    /// <summary>
    /// Reads the directional input from the player to determine the direction input for the combo.
    /// </summary>
    /// <param name="ctx"></param>
    public void ReadDirectional(InputAction.CallbackContext ctx)
    {
        _directionalInput = ctx.ReadValue<Vector2>();
    }

    /// <summary>
    /// Separate timer for the wheel to pop up after the player collects elemental ammo
    /// </summary>
    /// <param name="element">Element object to set active for the effect</param>
    /// <returns></returns>
    private IEnumerator WheelPopup(int element)
    {
        _elementalWheelRef.position = new Vector2(transform.position.x,transform.position.y + _capsuleOffset);
        _elementalWheelRef.GetChild(element - 1).gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        _elementalWheelRef.GetChild(element - 1).gameObject.SetActive(false);
    }

    /// <summary>
    /// Used for Dash X for the player to ignore enemies while dashing through them.
    /// </summary>
    /// <returns></returns>
    private IEnumerator IgnoreCollisiononPropulsion()
    {
        Physics2D.IgnoreLayerCollision(PLAYERLAYER,ENEMYLAYER, true);
        yield return new WaitForSeconds(0.1f);
        Physics2D.IgnoreLayerCollision(PLAYERLAYER, ENEMYLAYER, false);
    }

    /// <summary>
    /// Reads directional input and converts it to a character for the combo string.
    /// </summary>
    /// <returns></returns>
    char? DirectionalInput()
    {
        Debug.Log("Directional called");
        if(_isSprinting)
        {
            Debug.Log("Returning dash output");
            return DASH;
        }
        // Prioritizes left/right for combo input
        if (Mathf.Abs(_directionalInput.x) > Mathf.Abs(_directionalInput.y))
        {
            if (_directionalInput.x > 0)
                return RIGHT;
            else
                return LEFT;
        }
        else if(Mathf.Abs(_directionalInput.x) < Mathf.Abs(_directionalInput.y))
        {
            if (_directionalInput.y > 0)
                return UP;
            else
                return DOWN;
        }
        // If somehow not a valid character, pass in null
        return null;
    }

    /// <summary>
    /// Add light attack to combo string
    /// </summary>
    /// <param name="ctx">Input action value</param>
    public void LightAttackAction(InputAction.CallbackContext ctx)
    {
        AddToComboString(LIGHT);
    }

    /// <summary>
    /// Add heavy attack to combo string
    /// </summary>
    /// <param name="ctx">Input action value</param>
    public void HeavyAttackAction(InputAction.CallbackContext ctx)
    {
        AddToComboString(HEAVY);

    }

    /// <summary>
    /// Difference from original is ability to use the attack buffer to continue the chain
    /// To give more responsiveness
    /// </summary>
    public override void Recover()
    {
        // If there's an active buffer, continue the chain.
        //The buffered input was already stored beforehand so no need to append the input char here.
        if (_attackBuffer)
        {
            Attack();
        }
        else if(!_inAir)
        {
            // If the player isn't in the air when this is called
            // Reset the chain to let the player follow up with whatever new move they have.
            if (_dropComboTimer != null)
            {
                StopCoroutine(_dropComboTimer);
            }
                _dropComboTimer = StartCoroutine(DropComboTimer());
            base.Recover();
        }
        else if (_inAir)
        {
            base.Recover();
        }
        
        // Due to the flipflop nature, if contact damage was enabled, disable it here.
        if(_canDealContactDamage)
            TriggerContactDamage();
            
    }

    /// <summary>
    /// Only matters if contact damage is enabled, checks to see if it's an enemy and deal damage
    /// if it is.
    /// </summary>
    /// <param name="collision">Collision data of the impact, preferabbly enemy</param>
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if(_canDealContactDamage && collision.collider.CompareTag("Enemy"))
        {
            Hit(collision.collider);
        }
    }

    /// <summary>
    /// Where the combo interpretation happens as the string is read, then passed into the dictionary
    /// to see if the combo is there.
    /// </summary>
    public override void Attack()
    {
        KateAttackData data = null;
        // Checks to see if the combo is there.
        if (ContainsCombo(_currentComboString))
        {
            Debug.Log("Starting attack string " + _currentComboString);
            data = _attackMap[_currentComboString];
        }
        else
        {
            Debug.Log("Couldn't find attack with string "+_currentComboString);
            _inAttack = false;
            _currentComboString = "";
            // If this was called during recovery, grant the actual recovery before removing the buffer.
            if (_attackBuffer)
                base.Recover();
            _attackBuffer = false;
            return;
        }

        _attackBuffer = false;
        _currentMove = data;
        _currentKateAttack = data;

        // If the player was sprinting, cancel the sprint.
        if(_isSprinting)
        {
            Debug.Log("Canceling sprint");
            _playerMovement.startSprinting?.Invoke(false);
            
                
        }

        // Shouldn't be null. If valid, evaluate.
        if (data != null)
        {
            // If end of combo string, reset the string to enable new combos.
            if (data.endOfChain)
                _currentComboString = "";
            
            // Was meant to be used for entering cannon mode on the special combo.
            if (EnterCannonMode(data.specialTag))
                base.EnterAttackState();
            // Calls upon the attack.
            base.Attack();
        }
    }

    /// <summary>
    /// Used in various attacks that consumes ammo. Depletes the ammo counter if they have ammo
    /// or return false if they are out of ammo. This is primarily used to check if the player has ammo.
    /// </summary>
    /// <returns>Was the ammo successfully used?</returns>
    public bool UseAmmo()
    {
        if (_ammo > 0)
        {
            Debug.Log("Consuming ammo");
            _lastUsedAmmoType = (AttackData.Element)_ammoLoaded[_ammo-1];
            _ammoLoaded[_ammo-1] = -1;
            _playerStatus.DepleteAmmo(_ammo - 1);
            _ammo--;
            return true;
        }
        else
            return false;
    }

    /// <summary>
    ///  Overloaded to handle various special actions such as elemental ammo load, dashing through enemies,
    ///   or double jumping with dash jump X.
    /// </summary>
    public override void SpecialAction()
    {

        if(_currentKateAttack.specialTag >= KateAttackData.SpecialKateTags.LoadAmmo && _currentKateAttack.specialTag <= KateAttackData.SpecialKateTags.LoadWaterAmmo)
        {
            if (_ammo < _maxAmmo)
            {
                int ammoType = (int)(_currentKateAttack.specialTag - KateAttackData.SpecialKateTags.LoadAmmo);
                Debug.Log("Loading ammo type: "+(AttackData.Element)ammoType);
                if(ammoType > 0)
                    StartCoroutine(WheelPopup(ammoType));
                // Int wise, Element is between 0 - 5 for neutral to element and the load ammo tags have a cluster of those tags
                // placed in the same order therefore: LoadFireAmmo(5) - LoadAmmo(4) = Element.Fire (1)
                _ammo++;
                _ammoLoaded[_ammo-1] = ammoType;
                _playerStatus.AddAmmo(_ammo - 1, (AttackData.Element)ammoType);
                
            }
            return;
        }

        switch (_currentKateAttack.specialTag)
        {
            case KateAttackData.SpecialKateTags.DashThrough:
                Vector2 finalPosition = Vector2.zero;
                // As this function cannot be reached outside of animation events, no concern is needed
                // for canceling it.
                StartCoroutine(DashThrough());
                break;
            case KateAttackData.SpecialKateTags.DoubleJump:
                // Only enable the double jump if the player has ammo to expend.
                if(UseAmmo())
                {
                    _movement.Launch(_currentMove.selfPropelForces);
                    FireProjectile(transform.up * -1f);
                }
                break;
        }
    }

    /// <summary>
    /// Timer to handle player dashing through enemies to fire the missile.
    /// </summary>
    /// <returns>Just returns seconds to have it handle across frames</returns>
    private IEnumerator DashThrough()
    {
        float maxDuration = 0.25f;// Get the current and max duration
        Physics2D.IgnoreLayerCollision(PLAYERLAYER, ENEMYLAYER, true);// Ignores the player and enemy layer
        _movement.Launch(_currentMove.selfPropelForces);// Uses the movement propulsion function to propel Kate.
        while(maxDuration > 0f)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            maxDuration -= Time.deltaTime;
        }
        Physics2D.IgnoreLayerCollision(PLAYERLAYER, ENEMYLAYER, false);
        Debug.Log("Fire!");
        // Fires projectile if Kate has ammo to spare.
        if (_ammo > 0)
        {
            if(UseAmmo())
            {
                FireProjectile(-1f * transform.localScale.x * transform.right);
            }
            
        }

    }

    /// <summary>
    /// Non animation event version of firing projectile, primarily used for Double Jump.
    /// </summary>
    /// <param name="direction"></param>
    public void FireProjectile(Vector2 direction)
    {
        ProjectileManager.Instance.FireProjectile(this, ProjectileManager.ProjectileType.KateMissile, direction, 50f, true,2f,_lastUsedAmmoType);
    }
    /// <summary>
    /// Animation event version, used for dash X and dash jump XX.
    /// </summary>
    /// <param name="force"></param>
    public override void FireProjectile(float force)
    {
        if (UseAmmo())
        {
            ProjectileManager.Instance.FireProjectile(this, ProjectileManager.ProjectileType.KateMissile, new Vector2(transform.localScale.x * _currentKateAttack.missileDirection.x, _currentKateAttack.missileDirection.y), force, true, 2f, _lastUsedAmmoType);
        }
    }


    /// <summary>
    ///  Went unused, meant to enter cannon mode if the tag has been received.
    /// </summary>
    /// <param name="tag"></param>
    /// <returns>Returns if the tag is the tag used to enter cannon mode.</returns>
    bool EnterCannonMode(KateAttackData.SpecialKateTags tag)
    {
        return tag == KateAttackData.SpecialKateTags.EnterCannonMode;
    }

    /// <summary>
    /// Checks if the combo is present.
    /// </summary>
    /// <param name="combo">combo string to check</param>
    /// <returns>Does the combo exist?</returns>
    bool ContainsCombo(string combo)
    {
        return _attackMap.ContainsKey(combo);
    }

    /// <summary>
    /// Checks if there is directional input and append it if there is.
    /// </summary>
    /// <param name="key">The directional input string.</param>
    public void CheckDirectional(char key)
    {
        char? directional;
        if (_directionalInput.magnitude > 0)// If there is movement in general
        {
            directional = DirectionalInput();
            // To avoid breaking the flow of the combo and easing annoyance of using for example
            // Forward + Z when you mean Z, the input will ignore the direction if no valid combo
            // exists using it.
            if (directional != null && ContainsCombo(_currentComboString + directional + key))
            {
                _currentComboString += directional;
            }
            else
            {
                Debug.Log("Invalid combo string: " + _currentComboString + directional + key);
            }
        }
    }

    /// <summary>
    /// Where the combo string gets built after all the layers of functions are gone through.
    /// </summary>
    /// <param name="key"></param>
    public void AddToComboString(char key)
    {
        if (_inAttack)
        {
            if (!_attackBuffer)// If the player is already attacking, cache the input for later.
            {
                _attackBuffer = true;
                if(!_inAir)
                {
                    CheckDirectional(key);
                }
                
                _currentComboString += key;
                return;
            }
            else
            {
                return;
            }
        }

        if(!_inAir)
            CheckDirectional(key);

        _currentComboString += key;
        Attack();
    }

}
