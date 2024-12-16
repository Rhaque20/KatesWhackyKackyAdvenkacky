using System;
using System.Collections;
using UnityEngine;

public class Movement : MonoBehaviour
{
    // Powers of 2s 2^3 and 2^6
    protected const int GROUNDLAYER = 8,PLATFORMLAYER = 64;
    [SerializeField] protected float _moveSpeed = 3f;
    [SerializeField] protected float _jumpPower = 300f;
    [SerializeField] protected float _groundTraceDistance = 1f;
    [SerializeField]private LayerMask _groundLayerMask;
    [SerializeField]protected bool _onGround = true, _hasLanded = true, _canMove = true;
    [SerializeField]protected bool _jumping = false, _overrideGravity = false;
    protected Rigidbody2D _rigidBody;
    protected float _xDir = 0f;
    protected Animator _anim;
    protected CapsuleCollider2D _capsuleCollider;

    protected ContactFilter2D _contactFilter;
    protected RaycastHit2D[] _hitResults;

    protected Coroutine _suspendGroundCheck = null;
    // Primarily used by attacks
    protected Action<bool> _disableMobility;

    protected float _speedModifier = 1f;

    Collider2D _ignoredPlatform = null;

    // Used for going down platforms.
    private Coroutine _ignorePlatformTimer = null;

    public Action<bool> disableMobility
    {
        get { return _disableMobility; }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    /// <summary>
    /// Initializes the component
    /// </summary>
    protected virtual void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _anim = transform.GetChild(0).GetComponent<Animator>();
        _capsuleCollider = GetComponent<CapsuleCollider2D>();
        // Layer 3 has ground mask therefore 2^3 = 8
        _groundLayerMask = GROUNDLAYER;
        _groundLayerMask += PLATFORMLAYER;

        _contactFilter.layerMask = _groundLayerMask;
        _disableMobility += SetMove;
    }

    /// <summary>
    /// Called from disable mobility delegate.
    /// </summary>
    /// <param name="moveSet"></param>
    public void SetMove(bool moveSet)
    {
        _canMove = moveSet;
        _overrideGravity = false;
    }

    /// <summary>
    /// Used to avoid having the ground be checked on the raycast while the player is ascending
    /// </summary>
    /// <returns></returns>
    protected IEnumerator SuspendGroundCheckTimer()
    {
        yield return new WaitForSeconds(0.1f);
        _suspendGroundCheck = null;
    }

    /// <summary>
    /// Used to perform the movement action itself for both player and enemy.
    /// </summary>
    public void PerformMove()
    {

        if((_xDir > 0f || _xDir < 0f) && _canMove)
        {
            int sign = (int)Mathf.Sign(_xDir);
            //_rigidBody.MovePosition(_rigidBody.position + (_moveSpeed * Time.deltaTime * new Vector2(_xDir, _rigidBody.position.y)));
            _anim.SetInteger("MoveIntensity",sign);
            transform.localScale = new Vector2(sign,transform.localScale.y);
            _rigidBody.linearVelocity = new Vector2(_xDir * _moveSpeed * _speedModifier, _rigidBody.linearVelocity.y);
        }
        else if(!_overrideGravity)
        {
            _rigidBody.linearVelocity = new Vector2(0f, _rigidBody.linearVelocity.y);
            _anim.SetInteger("MoveIntensity", 0);
        }    
    }

    /// <summary>
    /// Does the jump for player and enemy.
    /// </summary>
    public virtual void Jump()
    {
        // Just to avoid being able to jump prematurely
        if(_onGround && _suspendGroundCheck == null)
        {
            _anim.Play("Jump");
            Debug.Log("Hop!");
            _rigidBody.linearVelocityY = 0;
            _rigidBody.AddForce(new Vector2(_xDir, _jumpPower));
            _suspendGroundCheck = StartCoroutine(SuspendGroundCheckTimer());
            _onGround = false;
            _hasLanded = false;
        }
    }

    /// <summary>
    /// To be expanded to handle logic for when the player hits the ground after jumping or falling.
    /// Will later expand to have hasLanded be false if the player is off the ground without jumping (falling)
    /// </summary>
    public virtual void Landed()
    {
        Debug.Log("Landed");
        _hasLanded = true;
        if (_ignorePlatformTimer != null)
        {
            StopCoroutine(_ignorePlatformTimer);
            Physics2D.IgnoreCollision(_capsuleCollider, _ignoredPlatform, false);
        }

    }
    
    /// <summary>
    /// Launches entity and ignores the movement gravity parameters to do so.
    /// </summary>
    /// <param name="launchPower">Direction of launch</param>
    public void Launch(Vector2 launchPower)
    {
        _overrideGravity = true;
        _rigidBody.linearVelocityY = 0f;
        _rigidBody.AddForce(new Vector2(launchPower.x * transform.localScale.x, launchPower.y) * _rigidBody.mass, ForceMode2D.Impulse);
    }

    /// <summary>
    /// Timer that counts down so player can fall through the platform they're on.
    /// </summary>
    /// <returns></returns>
    private IEnumerator PlatformIgnoreTimer()
    {
        yield return new WaitForSeconds(1f);
        Physics2D.IgnoreCollision(_capsuleCollider, _ignoredPlatform, false);
        _ignorePlatformTimer = null;
    }

    /// <summary>
    /// Called variably (input action or AI) to fall through the current platform if they are on one.
    /// </summary>
    /// <returns></returns>
    public bool FallThruPlatform()
    {
        if (!_canMove)
            return false;


        RaycastHit2D result = Physics2D.Raycast(transform.position, Vector2.down, _groundTraceDistance, PLATFORMLAYER);
        if (result && _ignorePlatformTimer == null)
        {
            _ignoredPlatform = result.collider;
            Physics2D.IgnoreCollision(_capsuleCollider, _ignoredPlatform, true);
            _ignorePlatformTimer = StartCoroutine(PlatformIgnoreTimer());
        }

        return true;
    }

    /// <summary>
    /// To be called during fixed intervals for physics calculations
    /// </summary>
    public virtual void FixedUpdate()
    {
        PerformMove();

        if(_suspendGroundCheck == null)
            _onGround = Physics2D.Raycast(transform.position, Vector2.down, _groundTraceDistance,_groundLayerMask);

        if(_onGround && !_hasLanded)
        {
            Landed();
        }

        if (_jumping && _canMove)
            Jump();

        if(!_onGround)
            _anim.SetInteger("AirVelocity",(int)Mathf.Sign(_rigidBody.linearVelocity.y));
        else
            _anim.SetInteger("AirVelocity", 0);

        _anim.SetBool("OnGround", _onGround);
    }
}
