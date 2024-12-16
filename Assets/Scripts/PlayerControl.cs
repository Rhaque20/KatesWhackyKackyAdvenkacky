using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerControl : MonoBehaviour
{

    // Input action maps
    [SerializeField]private BaseControlScheme _controlDetails;
    private BaseControlScheme.InBattleActions _battleActions;
    private PlayerInput _playerInput;
    private KateCore _kateCore;

    private PlayerMovement _playerMovement;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Sets up and binds all the necessary functions to the input actions for the player.
    void Start()
    {
        // Sets up the player input from the Enhanced input system
        _playerInput = GetComponent<PlayerInput>();
        _playerMovement = GetComponent<PlayerMovement>();
        _kateCore = GetComponent<KateCore>();

        _controlDetails = new BaseControlScheme();

        // Enables the inbattle control scheme
        _controlDetails.InBattle.Enable();

        // Binds functions to input delegates.
        _controlDetails.InBattle.Move.performed += _playerMovement.ReceiveMoveInput;
        _controlDetails.InBattle.Move.canceled += _playerMovement.ReceiveMoveInput;

        _controlDetails.InBattle.Move.performed += _kateCore.ReadDirectional;
        _controlDetails.InBattle.Move.canceled += _kateCore.ReadDirectional;

        _controlDetails.InBattle.Jump.performed += _playerMovement.Jump;
        _controlDetails.InBattle.Jump.canceled += _playerMovement.ReleaseJump;

        _controlDetails.InBattle.HardFall.performed += _playerMovement.HardFall;
        _controlDetails.InBattle.HardFall.performed += _playerMovement.FallThruPlatform;

        _controlDetails.InBattle.Sprint.performed += _playerMovement.TriggerSprint;

        _controlDetails.InBattle.LightAttack.performed += _kateCore.LightAttackAction;
        _controlDetails.InBattle.HeavyAttack.performed += _kateCore.HeavyAttackAction;

        _controlDetails.InBattle.UseHeal.performed += _kateCore.EatTurkey;

        LevelManager.Instance.sceneChangeFunctions += DisableControls;

    }

    /// <summary>
    /// Called before a scene change to avoid memory leaks.
    /// </summary>
    public void DisableControls()
    {
        _controlDetails.InBattle.Disable();
    }
}
