using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    // Owning system reference to refer back to when player enters the checkpoint
    CheckpointSystem _owningSystem;
    BoxCollider2D _trigger;
    // Special events to play when the checkpoint has been entered.
    public enum TriggerEffect { None, SpawnEnemyGroup, OpenDoors, SpawnEnemyWave }
    [SerializeField] TriggerEffect _effectOnCheckpoint;
    [SerializeField] SpriteRenderer _spriteRenderer;
    [SerializeField] Sprite _activatedFlag;

    /// <summary>
    /// Called by the checkpoint system to set up the owning system and standard start function
    /// initializations
    /// </summary>
    /// <param name="system">Owning checkpoint system reference that will be passed in</param>
    public void Initialize(CheckpointSystem system)
    {
        _owningSystem = system;
        _trigger = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// To be fully called when the player enters the checkpoint
    /// in order to set the checkpoint of the current level to that when the player goes out of bounds.
    /// </summary>
    /// <param name="other">Collider that enters the trigger, preferrably the player</param>
    public void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            // Updates the checkpoint index based on this sibling index as a reference.
            _owningSystem.UpdateCheckPoint(transform.GetSiblingIndex());
            // Trigger any checkpoint effects
            LevelManager.Instance.TriggerAtCheckpoint(_effectOnCheckpoint);
            // Prevent the trigger from being retriggerable.
            _trigger.enabled = false;
            // Set the checkpoint flag to happy :D
            _spriteRenderer.sprite = _activatedFlag;
        }
    }
}
