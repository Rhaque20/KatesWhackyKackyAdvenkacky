using UnityEngine;

[RequireComponent (typeof(BoxCollider2D))]
public class CheckpointSystem : MonoBehaviour
{
    // Reference to the player, checkpoint list (aka the transform with checkpoint children)
    // and any deathplane references.
    [SerializeField] GameObject _playerRef;
    [SerializeField] Transform _checkPointRef;
    [SerializeField] Transform _deathPlanes;
    int _currentCheckPoint = 0;

    /// <summary>
    /// Does all the preinitialization before all Start functions here namely
    /// getting player ref and initializing all the checkpoints and deathplanes.
    /// </summary>
    private void Awake()
    {
        // Using this is faster than standard GameObject.Find as it uses the built in dictionary indexing
        // unity has with game objects.
        _playerRef = GameObject.FindGameObjectWithTag("Player");
        if (_checkPointRef != null) // Initialize all checkpoints if the ref isn't null
        {
            for (int i = 0; i < _checkPointRef.childCount; i++)
            {
                _checkPointRef.GetChild(i).GetComponent<Checkpoint>().Initialize(this);
            }
        }

        if (_deathPlanes != null)// Initialize all the deathplanes if the ref isn't null.
        {
            for (int i = 0; i < _deathPlanes.childCount; i++)
            {
                _deathPlanes.GetChild(i).GetComponent<DeathPlane>().Initialize(this);
            }
        }
    }

    /// <summary>
    /// Called when the player enters a deathplane, causing them to teleport back to the checkpoint.
    /// The currentcheckpoint index is based on the childindex of the checkpoint references.
    /// </summary>
    public void RespawnAtCheckPoint()
    {
        _playerRef.transform.position = _checkPointRef.GetChild(_currentCheckPoint).transform.position;
    }

    /// <summary>
    /// Update the current index of checkpoints based on the index passed in.
    /// </summary>
    /// <param name="checkPointIndex">Sibling index of the checkpoint to get their location</param>
    public void UpdateCheckPoint(int checkPointIndex)
    {
        _currentCheckPoint = checkPointIndex;
    }


}
