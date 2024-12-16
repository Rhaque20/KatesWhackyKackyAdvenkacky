using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : Movement
{
    [SerializeField] private float _hardFallVelocity = 10f;
    [SerializeField] private float _sprintModifier = 1.4f;
    [SerializeField] private bool _isSprinting = false;
    [SerializeField] private AnimationClip _walkAnim, _runAnim;
    [SerializeField] AnimatorOverrideController _animOverride;
    protected Action<bool> _startSprinting, _startJump;
    private SoundManager _soundManager;
    

    private bool _bindedSprint = false;

    /// <summary>
    /// Getter of startsprinting.
    /// </summary>
    public Action<bool> startSprinting
    {
        get { return _startSprinting; }
    }

    /// <summary>
    /// Getter of isSprinting.
    /// </summary>
    public bool isSprinting
    {
        get { return _isSprinting; }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    /// <summary>
    /// Set up the animation override controller and bind the sprint function to the delegate
    /// for combo purposes.
    /// </summary>
    protected override void Start()
    {
        base.Start();
        _animOverride = (AnimatorOverrideController)transform.GetChild(0).GetComponent<Animator>().runtimeAnimatorController;
        _startSprinting += TriggerSprint;
        _animOverride["Walk"] = _walkAnim;
        _soundManager = GetComponent<SoundManager>();

    }

    /// <summary>
    /// Used to help set up the subscription/binding of sprinting.
    /// </summary>
    /// <param name="sprintFunction">The function address to bind</param>
    public void SetUpSprintFunction(Action<bool> sprintFunction)
    {
        Debug.Log("Binded");
        _startSprinting += sprintFunction;
        
        Debug.Log("Successfully binded "+_startSprinting.GetInvocationList().Length);
    }

    /// <summary>
    /// Just like sprint but for jumps
    /// </summary>
    /// <param name="jumpFunction">function address to bind</param>
    public void SetUpJumpFunction(Action<bool> jumpFunction)
    {
        _startJump += jumpFunction;
    }

    /// <summary>
    /// Handles the movement input and translates it into a float for horizontal movement
    /// </summary>
    /// <param name="ctx">The input data received</param>
    public void ReceiveMoveInput(InputAction.CallbackContext ctx)
    {
        float xInput = ctx.ReadValue<Vector2>().x;

        _xDir = xInput;
    }

    /// <summary>
    /// Used to trigger the sprint based on sprint input action.
    /// </summary>
    /// <param name="ctx">Input action value</param>
    public void TriggerSprint(InputAction.CallbackContext ctx)
    {
        _startSprinting?.Invoke(!_isSprinting);
    }

    /// <summary>
    /// The actual logic for setting sprint value and changing animations accordingly.
    /// </summary>
    /// <param name="sprintVal"></param>
    public void TriggerSprint(bool sprintVal)
    {
        _isSprinting = sprintVal;
        if (_isSprinting)
        {
            _speedModifier *= _sprintModifier;
            _animOverride["Walk"] = _runAnim;
        }
        else
        {
            _speedModifier /= _sprintModifier;
            _animOverride["Walk"] = _walkAnim;
        }
    }

    /// <summary>
    /// Used to trigger the generic jump from the movement class in input
    /// </summary>
    /// <param name="ctx">The input data received</param>
    public void Jump(InputAction.CallbackContext ctx)
    {
        _jumping = true;
    }

    /// <summary>
    /// Used for jumping, making sure the player isn't already jumping.
    /// </summary>
    public override void Jump()
    {
        if(_canMove)
        {
            if (_suspendGroundCheck == null && _onGround)
            {
                _soundManager.PlayJump();
                Debug.Log("Suspending ground check and passing that info to core");
                _startJump?.Invoke(true);
            }
            base.Jump();
        }
        
        
    }

    /// <summary>
    /// Used for when the player hits the ground.
    /// </summary>
    public override void Landed()
    {
        base.Landed();
        _startJump?.Invoke(false);
    }

    /// <summary>
    ///  Used for auto jumping purposes.
    /// </summary>
    /// <param name="ctx">Input action value</param>
    public void ReleaseJump(InputAction.CallbackContext ctx)
    {
        _jumping = false;
    }

    /// <summary>
    /// Forces the player downward on the ground which is useful for rapid falling
    /// To extend combos or avoid an attack as you're falling easier.
    /// </summary>
    /// <param name="ctx">Input action vlaue.</param>
    public void HardFall(InputAction.CallbackContext ctx)
    {
        if(!_onGround && _canMove)
            _rigidBody.linearVelocity = new Vector2(_rigidBody.linearVelocity.x, -_hardFallVelocity);
    }
    /// <summary>
    /// Fall through the current platform if on one.
    /// </summary>
    /// <param name="ctx">Input action value</param>
    public void FallThruPlatform(InputAction.CallbackContext ctx)
    {
        FallThruPlatform();
    }
}
