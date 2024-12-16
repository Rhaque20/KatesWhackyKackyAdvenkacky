using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] protected bool _isFriendly = false;
    [SerializeField] protected LayerMask _hitMask;
    [SerializeField] private Gradient[] _missileTrails = new Gradient[5];
    AudioSource _audioSrc;
    Rigidbody2D _rigid;
    BoxCollider2D _boxCollider;
    EntityCore _owner;
    Coroutine _projectileLifeTimer = null;
    SpriteRenderer _spriteRenderer;
    TrailRenderer _trail;
    ProjectileManager.ProjectileType _projectileType;
    float _blastRadius = 1f;
    [SerializeField]AttackData.Element _affinity = AttackData.Element.Neutral;
    GameObject[] _hitImpacts;

    public void Start()
    {
        _rigid = GetComponent<Rigidbody2D>();
        _boxCollider = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _hitImpacts = new GameObject[transform.childCount];
        _trail = GetComponent<TrailRenderer>();
        _audioSrc = GetComponent<AudioSource>();
        for(int i = 0; i < _hitImpacts.Length; i++)
        {
            _hitImpacts[i] = transform.GetChild(i).gameObject;
            _hitImpacts[i].SetActive(false);
        }
    }

    public virtual void Initialize(EntityCore owner, bool isFriendly, ProjectileManager.ProjectileType type)
    {
        _owner = owner;
        _isFriendly = isFriendly;
        _projectileType = type;
    }

    public void Launch(Vector2 direction,float force, float blastRadius = 0f, AttackData.Element affinity = AttackData.Element.Neutral, float lifeTime = 1f)
    {
        gameObject.SetActive(true);
        _boxCollider.enabled = true;
        _spriteRenderer.enabled = true;
        _rigid.AddForce(direction * force, ForceMode2D.Impulse);
        _blastRadius = blastRadius; 
        _projectileLifeTimer = StartCoroutine(ProjectileLife(lifeTime));
        _affinity = affinity;
        _trail.colorGradient = _missileTrails[(int)_affinity];
        Debug.Log("Setting affinity to " + _affinity);
    }

    private IEnumerator ExplosionDuration()
    {
        if(_affinity > AttackData.Element.Neutral)
        {
            _hitImpacts[(int)_affinity - 1].transform.localScale = new Vector2(_blastRadius, _blastRadius);
            _hitImpacts[(int)_affinity - 1].SetActive(true);
        }
        _audioSrc.Play();

        _boxCollider.enabled = false;
        _spriteRenderer.enabled = false;

        yield return new WaitForSeconds(0.3f);

        if (_affinity > AttackData.Element.Neutral)
            _hitImpacts[(int)_affinity - 1].SetActive(false);

        ProjectileManager.Instance.ReturnProjectile(_projectileType, this);
    }

    public virtual void RemoveProjectile()
    {
        _rigid.linearVelocity = Vector2.zero;
        if(_projectileLifeTimer != null )
        {
            StopCoroutine( _projectileLifeTimer );
        }
        if (_blastRadius > 0f)
            Explode();

        StartCoroutine(ExplosionDuration());
    }

    public virtual void Explode()
    {
        RaycastHit2D[] hitResult = Physics2D.CircleCastAll(transform.position, _blastRadius, Vector2.zero, 0f, _hitMask);

        if(hitResult.Length > 0 )
        {
            foreach(RaycastHit2D hitTarget in hitResult)
            {
                if (_isFriendly && hitTarget.collider.CompareTag("Enemy"))
                {
                    Debug.Log(hitTarget.collider.name + " got hit in the collateral");
                    _owner.Hit(hitTarget.collider);
                }
                else if (!_isFriendly && hitTarget.collider.CompareTag("Player"))
                {
                    _owner.Hit(hitTarget.collider);
                }
            }
        }

    }

    protected IEnumerator ProjectileLife(float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        _projectileLifeTimer = null;
        RemoveProjectile();
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if(_isFriendly)
        {
            if(other.CompareTag("Enemy"))
            {
                _owner.Hit(other);
                RemoveProjectile();
                return;
            }
        }
        else
        {
            if (other.CompareTag("Player"))
            {
                _owner.Hit(other);
                RemoveProjectile();
                return;
            }
        }

        if(other.CompareTag("Ground"))
        {
            RemoveProjectile();
        }

        Debug.Log("Projectile Hit otherwise: " + other.name);
    }
}
