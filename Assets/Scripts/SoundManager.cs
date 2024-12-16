using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] AudioSource _hitSource, _jumpSource;

    public void PlayHit()
    {
        _hitSource.Play();
    }

    public void PlayJump()
    {
        _jumpSource.Play();
    }
}
