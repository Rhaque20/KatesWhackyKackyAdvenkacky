using System.Collections;
using UnityEngine;

public class PlasmaBursterCore : EnemyCore
{
    [SerializeField] int _maxAmmoCapacity = 20;
    int _ammoCapacity = 20;

    /// <summary>
    /// Meant to initialize the ammo count as it was meant to have ranged capability.
    /// </summary>
    public void Start()
    {
        base.Start();
        _ammoCapacity = _maxAmmoCapacity;
    }
    /// <summary>
    /// Meant for cannon mode of burster.
    /// </summary>
    public override void EnterAttackState()
    {
        base.EnterAttackState();
        if (_inAttackState)
            StartCoroutine(GattlingMode());
    }

    /// <summary>
    /// Also meant for cannon mode of burster.
    /// </summary>
    /// <returns></returns>
    private IEnumerator GattlingMode()
    {
        while (_ammoCapacity > 0)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            _ammoCapacity--;
        }

        _ammoCapacity = _maxAmmoCapacity;
        EnterAttackState();
    }
}
