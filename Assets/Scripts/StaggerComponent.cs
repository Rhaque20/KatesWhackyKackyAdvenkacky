using System;
using System.Collections;
using UnityEngine;

public class StaggerComponent : MonoBehaviour
{
    [SerializeField] protected int _stoicThreshold = 0;
    [SerializeField] protected float _stoicTime = 2f;
    protected int _stoicCount = 0;
    protected Action<bool> _onInterrupt;

    protected Coroutine _stoicTimer = null, _interruptTimer = null;

    public bool inStoic
    {
        get { return _stoicTimer != null; }
    }

    private void Start()
    {
        _onInterrupt += GetComponent<Movement>().SetMove;
        _onInterrupt += GetComponent<EntityCore>().InterruptAction;
    }

    protected IEnumerator StoicTimer()
    {
        yield return new WaitForSeconds(_stoicTime);
        _stoicTimer = null;
        Debug.Log("Stoic ended");
    }

    protected IEnumerator InterruptTimer()
    {
        yield return new WaitForSeconds(1f);
        _onInterrupt?.Invoke(true);
        _interruptTimer = null;
    }

    private void OnDisable()
    {
        if (_stoicTimer != null)
        {
            StopCoroutine( _stoicTimer );
        }

        if(_interruptTimer != null)
        {
            StopCoroutine( _interruptTimer );
        }
    }

    public void BreakOutofStagger()
    {
        if(_interruptTimer != null)
        {
            StopCoroutine(_interruptTimer );
        }
        _interruptTimer = null;

        _onInterrupt?.Invoke(true);
    }

    public void ReceiveHit()
    {
        if(!inStoic)
        {
            if (_stoicThreshold > 0)
            {
                _stoicCount++;
                if (_stoicCount == _stoicThreshold)
                {
                    Debug.Log("STOIC ACTIVATE!");
                    BreakOutofStagger();
                    if(gameObject.activeSelf)
                        _stoicTimer = StartCoroutine(StoicTimer());
                    _stoicCount = 0;
                }
            }

            if(_stoicTimer == null && _interruptTimer == null)
            {
                _onInterrupt?.Invoke(false);
                if (gameObject.activeSelf)
                    _interruptTimer = StartCoroutine(InterruptTimer());

            }
        }
        
    }
}
