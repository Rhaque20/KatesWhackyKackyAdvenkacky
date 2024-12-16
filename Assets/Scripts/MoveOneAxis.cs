using System.Collections;
using UnityEngine;

public class MoveOneAxis : MonoBehaviour
{
    [SerializeField, Tooltip("Determines how far away on both sides of the axes it can move")] float _displacementRange = 2f;
    [SerializeField] bool _moveOnXAxis = true;
    [SerializeField] float _platformSpeed = 1f;
    [SerializeField] PlatformType _platformType;
    // It seems hitting top is -1 on y axis and left is 1 on x axis
    [SerializeField] Vector2 _acceptableRange = new Vector2(0,-1f);
    [Header("DamagePlane Settings")]
    float _damageCooldown = 0.5f;
    private enum PlatformType { None, MovingPlatform, DamagePlane}
    Vector2 _startingPosition;
    [SerializeField]float _positionDelta = 0.5f;
    bool _goPositive = true;
    bool _canAttach = false;

    Coroutine _damageCoolDownTimer = null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    /// <summary>
    /// Sets up the initial position.
    /// </summary>
    void Start()
    {
        _startingPosition = transform.position;
    }

    /// <summary>
    /// If used as moving platform, used to check if player or enemy is on it and parent them
    /// so they move with the platform.
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(_platformType == PlatformType.MovingPlatform)
        {
            if(collision.collider.CompareTag("Player") || collision.collider.CompareTag("Enemy"))
            {
                ContactPoint2D contact = collision.GetContact(0);
                Debug.Log("Contact point at this is " + contact.normal);
                if(contact.normal == _acceptableRange)// Used to make sure they land on top of the platform and not on the side or bottom.
                    collision.transform.SetParent(transform, true);
            }
        }
    }

    /// <summary>
    /// Used for damage plane to avoid damage per frame
    /// </summary>
    /// <returns></returns>
    private IEnumerator DamageCooldownTimer()
    {
        yield return new WaitForSeconds(_damageCooldown);
        _damageCoolDownTimer = null;
    }

    /// <summary>
    /// Used for damage plane to check if player is in it and deal damage to them.
    /// </summary>
    /// <param name="collision">Collider, preferably player</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_platformType == PlatformType.DamagePlane)
        {
            if (collision.CompareTag("Player"))
            {
                if(_damageCoolDownTimer == null)
                {
                    collision.GetComponent<Health>().ReceiveDamage(10f, true);
                    _damageCoolDownTimer = StartCoroutine(DamageCooldownTimer());
                }
                
            }
        }
    }

    /// <summary>
    /// Ditto for OnTriggerEnter
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (_platformType == PlatformType.DamagePlane)
        {
            if (collision.CompareTag("Player"))
            {
                if (_damageCoolDownTimer == null)
                {
                    collision.GetComponent<Health>().ReceiveDamage(10f, true);
                    _damageCoolDownTimer = StartCoroutine(DamageCooldownTimer());
                }

            }
        }
    }

    /// <summary>
    /// used for moving platform to deparent the rider of the platform.
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (_platformType == PlatformType.MovingPlatform)
        {
            if (collision.collider.CompareTag("Player") || collision.collider.CompareTag("Enemy"))
            {
                collision.transform.SetParent(null, true);
            }
        }
    }

    // Update is called once per frame
    /// <summary>
    /// Used for the movement logic, going back and forth using lerps.
    /// </summary>
    private void FixedUpdate()
    {
        if (_moveOnXAxis)
        {
            transform.position = new Vector2(Mathf.Lerp(_startingPosition.x + _displacementRange, _startingPosition.x - _displacementRange, _positionDelta), transform.position.y);
        }
        else
        {
            transform.position = new Vector2(_startingPosition.x, Mathf.Lerp(_startingPosition.y + _displacementRange, _startingPosition.y - _displacementRange, _positionDelta));
        }
        if(_goPositive)
            _positionDelta += (Time.deltaTime * _platformSpeed);
        else
            _positionDelta -= (Time.deltaTime * _platformSpeed);

        if(_positionDelta > 1f)
        {
            _goPositive = false;
        }
        else if (_positionDelta < 0f)
        {
            _goPositive = true;
        }

    }
}
