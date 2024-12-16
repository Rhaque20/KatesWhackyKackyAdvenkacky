using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : Health
{
    const int MAXAMMO = 6;
    [SerializeField] Transform _ammoTracker, _turkeyTracker;
    [SerializeField] Color[] _elementColors = new Color[MAXAMMO];
    Image[] _ammoIcons;
    Coroutine _mercyTimer = null;

    private void Start()
    {
        base.Start();
        _ammoIcons = new Image[MAXAMMO];
        for (int i = 0; i < MAXAMMO; i++)
        {
            _ammoIcons[i] = _ammoTracker.GetChild(i).GetComponent<Image>();
        }
    }

    private IEnumerator ImmunityTimer()
    {
        yield return new WaitForSeconds(1f);
        _mercyTimer = null;
    }

    public override void ReceiveDamage(float damage, bool isMagic)
    {
        if (_mercyTimer == null)
        {
            base.ReceiveDamage(damage, isMagic);
            _mercyTimer = StartCoroutine(ImmunityTimer());
        }

    }

    public void AddAmmo(int index, AttackData.Element element)
    {
        _ammoIcons[index].gameObject.SetActive(true);
        _ammoIcons[index].color = _elementColors[(int)element];
    }
    public void DepleteAmmo(int index)
    {
        _ammoIcons[index].gameObject.SetActive(false);
    }

    public void AddTurkey(int currentCapacity)
    {
        _turkeyTracker.GetChild(currentCapacity-1).gameObject.SetActive(true);
    }

    public void RemoveTurkey(int currentCapacity)
    {
        _turkeyTracker.GetChild(currentCapacity + 1).gameObject.SetActive(false);
    }

    public override void Death()
    {
        base.Death();
        LevelManager.Instance.GameOver();
    }
}
