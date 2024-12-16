using UnityEngine;

public class AttackData : ScriptableObject
{
    public enum Element { Neutral ,Fire, Wind, Earth, Water}

    [SerializeField] protected float _modifier = 1.0f;
    [SerializeField] protected Element _affinity = Element.Neutral;
    [SerializeField] protected AnimationClip[] _animations = new AnimationClip[2];
    [SerializeField] protected Vector2 _knockbackForces = Vector2.zero, _selfPropelForces = Vector2.zero;

    /// <summary>
    /// Used to determine how much of the attack stat to use for attacks.
    /// </summary>
    public float modifier
    { 
        get { return _modifier; } 
    }

    /// <summary>
    /// Elemental affinity of attack, went unused mechanically but projectiles do have affinity.
    /// </summary>
    public Element affinity
    { 
        get { return _affinity; } 
    }

    /// <summary>
    /// Animations tied to the skill to be used with the animator override to freely
    /// change animations of two states to avoid relying on multiple states for the same two state attack.
    /// </summary>
    public AnimationClip[] animations
    {
        get { return _animations; }
    }

    /// <summary>
    /// Forces used to propel the attacker
    /// </summary>
    public Vector2 selfPropelForces
    {
        get { return _selfPropelForces; }
    }

    /// <summary>
    /// Used for knocking back enemies
    /// </summary>
    public Vector2 knockbackForces
    {
        get { return _knockbackForces; }
    }

}
