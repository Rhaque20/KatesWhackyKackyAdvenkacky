using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager Instance;
    public enum ProjectileType { KateMissile,KateShuriken,Plasmaball,EnemyMissle};
    [SerializeField] private GameObject _kateMissile, _kateShuriken;
    [SerializeField] int _missileCount = 10;
    Dictionary<ProjectileType, Queue<GameObject>> _projectilePool = new Dictionary<ProjectileType, Queue<GameObject>>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        _projectilePool.Add(ProjectileType.KateMissile,new Queue<GameObject>());
        _projectilePool.Add(ProjectileType.KateShuriken,new Queue<GameObject>());

        GameObject temp;

        for (int i = 0; i < _missileCount; i++)
        {
            temp = Instantiate(_kateMissile);
            _projectilePool[ProjectileType.KateMissile].Enqueue(temp);
            temp.transform.SetParent(transform);
            temp.SetActive(false);
            temp.GetComponent<Projectile>().Start();

            temp = Instantiate(_kateShuriken);
            _projectilePool[ProjectileType.KateShuriken].Enqueue(temp);
            temp.transform.SetParent(transform);
            temp.SetActive(false);
            temp.GetComponent<Projectile>().Start();
        }
    }

    public void FireProjectile(EntityCore owner,ProjectileType type,Vector2 direction, float force,bool friendly, float blastRadius = 0f, AttackData.Element attackAttribute = AttackData.Element.Neutral)
    {
        if (_projectilePool[type].Count > 0)
        {
            Projectile projectileRef = _projectilePool[type].Dequeue().GetComponent<Projectile>();
            projectileRef.transform.position = (Vector2)owner.transform.position + new Vector2(0f,0.5f);
            projectileRef.Initialize(owner, friendly, type);
            projectileRef.Launch(direction,force,blastRadius,attackAttribute);
        }
    }

    public void ReturnProjectile(ProjectileType type, Projectile projRef)
    {
        _projectilePool[type].Enqueue(projRef.gameObject);
        projRef.gameObject.SetActive(false);
    }
}
