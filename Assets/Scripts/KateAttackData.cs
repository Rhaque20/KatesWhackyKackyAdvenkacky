using UnityEngine;

[CreateAssetMenu(fileName = "KateAttack", menuName = "ScriptableObjects/KateAttackData", order = 1)]
public class KateAttackData : AttackData
{
    [SerializeField] protected string _requiredComboString;// combo string required to call this attack
    [SerializeField] protected bool _canConsumeAmmo = false;// Determines if this requires a missile to use the move properly
    [SerializeField] protected bool _effectOnShuriken = false;// Unused but meant to give different effect based on shuriken type.
    [SerializeField] protected bool _endOfChain = false;// Is this attack the last of its chain?
    [SerializeField] Vector2 _missileDirection = Vector2.zero; // Direction the missile fires.

    // Used to identify the type of special action to perform.
    public enum SpecialKateTags {None, EnterCannonMode, EnterAirCannon,DoubleJump ,LoadAmmo, LoadFireAmmo,LoadWindAmmo,LoadEarthAmmo,LoadWaterAmmo, DashThrough };
    [SerializeField] private SpecialKateTags _specialTag;

    /// <summary>
    /// Getter of combo string
    /// </summary>
    public string requiredComboString
    {
        get { return _requiredComboString; }
    }

    /// <summary>
    /// Getter of end of chain.
    /// </summary>
    public bool endOfChain
    { 
        get { return _endOfChain; } 
    }
    /// <summary>
    /// Getter of special tag.
    /// </summary>
    public SpecialKateTags specialTag
    { 
        get { return _specialTag; } 
    }

    /// <summary>
    /// Getter of canConsumeAmmo
    /// </summary>
    public bool canConsumeAmmo
    {
        get { return _canConsumeAmmo; }
    }

    /// <summary>
    /// Getter of missile direction
    /// </summary>
    public Vector2 missileDirection
    {
        get { return _missileDirection; }
    }

}
