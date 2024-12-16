using System.Collections;
using UnityEngine;

public class SwitchSystem : MonoBehaviour
{
    [SerializeField] bool _retoggleable = false;
    [SerializeField] float _toggleCooldown = 2f;
    BoxCollider2D _collider;

    bool _isOn = false;

    public void Start()
    {
        Health _healthRef = GetComponent<Health>();

        if(_healthRef != null )
        {
            _healthRef.onDeath += TriggerSwitch;
        }   

        _collider = GetComponent<BoxCollider2D>();
    }

    private IEnumerator ToggleTimer()
    {
        _collider.enabled = false;
        yield return new WaitForSeconds(_toggleCooldown);
        _collider.enabled = true;
    }

    public void TriggerSwitch()
    {
        _isOn = true;

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(!_isOn);
        }

        if (_retoggleable)
        {
            StartCoroutine(ToggleTimer());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            TriggerSwitch();
        }
    }

    public void OnDisable()
    {
        if(_retoggleable)
            gameObject.SetActive(true);
    }
}
