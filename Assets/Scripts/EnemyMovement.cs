using System.Collections;
using UnityEngine;

public class EnemyMovement : Movement
{
    // Enemy variant of the movement that is more automated.
    // Destination point.
    protected Transform _targetTransform = null;
    // A minimum distance to have between player and enemy to prevent enemies constantly
    // going towards them.
    [SerializeField] protected float _bubbleDistance = 1f;
    // Minimum threshold of height different to consider jump or going down on platforms
    [SerializeField] protected float _heightDifferenceThreshold = 2f;
    // Used for offsetting with bubble distance.
    float _capsuleRadius = 0f;
    Coroutine _jumpCoolDown = null;

    /// <summary>
    /// Do initialize based on Movement and get radius of capsule collider.
    /// </summary>
    public void Start()
    {
        base.Start();
        _capsuleRadius = GetComponent<CapsuleCollider2D>().size.x;
    }
    /// <summary>
    ///  Does the movement here.
    /// </summary>
    public override void FixedUpdate()
    {
        DetermineMoveDirection();
        base.FixedUpdate();
    }

    /// <summary>
    /// Cooldown to prevent spam jumping or going down on platforms.
    /// </summary>
    /// <returns></returns>
    private IEnumerator JumpCooldown()
    {
        yield return new WaitForSeconds(5f);
        _jumpCoolDown = null;
    }

    /// <summary>
    /// Does move direction based on x distance of player. If there is no ref, just stand there
    /// like a potato.
    /// </summary>
    public void DetermineMoveDirection()
    {
        if (_targetTransform != null)
        {
            _xDir = _targetTransform.position.x - transform.position.x;
            // If the enemy is within bubble distance, halt their final movement.
            if (Mathf.Abs(_xDir) <= _bubbleDistance + _capsuleRadius)
            {
                _xDir = 0f;
            }
            else
            {
                _xDir = Mathf.Sign(_xDir);
            }
            // If they're on the ground, no active jump cooldown, and can move
            // evaluate a special movement procedure.
            if(_onGround && _jumpCoolDown == null && _canMove)
            {
                float heightDiff = transform.position.y - _targetTransform.position.y;
                Debug.Log("Height Diff is " + heightDiff);
                // If enemy is on higher elevation of player, try to go down through platform.
                if (heightDiff > _heightDifferenceThreshold)
                {
                    Debug.Log("Falling through platform to get you");
                    FallThruPlatform();
                    _jumpCoolDown = StartCoroutine(JumpCooldown());
                }
                // Else if enemy is on lower elevation than player, jump to try and land on a platform
                // with the player.
                else if (heightDiff < -_heightDifferenceThreshold)
                {
                    Debug.Log("Jumping through platforms to get to you");
                    Jump();
                    _jumpCoolDown = StartCoroutine(JumpCooldown());
                }
            }
                
        }

    }

    /// <summary>
    /// Change the target, was meant to be used for other special cases but not enough time.
    /// </summary>
    /// <param name="newTarget">New target to change to</param>
    public void ChangeTarget(Transform newTarget)
    {
        _targetTransform = newTarget;
    }
}
