using UnityEngine;

public class DeathPlane : MonoBehaviour
{
    // Gets the checkpoint system to refer to when the player comes in contact with this.
    CheckpointSystem _checkpointSystem;

    public void Initialize(CheckpointSystem system)
    {
        _checkpointSystem = system;
    }
    /// <summary>
    /// Will trigger the death plane effect with the entity collides with the collider (not trigger)
    /// If it's the player's, send them back to the checkpoint
    /// If it's an enemy, just yeet (kill) them.
    /// </summary>
    /// <param name="collision">Collision data that came in contact with this death plane</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.CompareTag("Player"))
        {
            _checkpointSystem.RespawnAtCheckPoint();
        }
        else if (collision.collider.CompareTag("Enemy"))
        {
            collision.gameObject.SetActive(false);
        }
    }
}
