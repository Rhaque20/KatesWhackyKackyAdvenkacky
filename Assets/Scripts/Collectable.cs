using UnityEngine;

public class Collectable : MonoBehaviour
{
    // Type of collectible (spelling be damned), the last three matter for the
    // Collectable requirement of the collectathan part while the first one
    // the turkey leg is more of a grab now use later heal pack.
    public enum CollectibleType { TurkeyLeg, Unicorny1,Unicorny2,Unicorny3}
    [SerializeField] CollectibleType _type = CollectibleType.TurkeyLeg;
    bool _collected = false;

    /// <summary>
    /// Checks if the current collectible is a unicorn and if it is, check if the player had
    /// collected it before in a previous session using player prefs.
    /// </summary>
    private void Start()
    {
        if (_type > CollectibleType.TurkeyLeg && PlayerPrefs.HasKey(_type.ToString()))
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Called upon player entering its trigger/collection range. Has wildly different effects
    /// based on the category.
    /// </summary>
    /// <param name="collision">The collider that entered, preferrably the player's</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (_type == CollectibleType.TurkeyLeg)
            {
                // Check to see if the player can collect the turkey (aka not at max capacity)
                // if so, collect it and hide the collectible to prevent recollection.
                if (collision.GetComponent<KateCore>().CollectTurkey())
                {
                    gameObject.SetActive(false);
                }
            }
            else
            {
                // Using the player preferences, data like this can persist between sessions
                // which can be extremely helpful if the player dies and has to restart the level
                // their unicorn progress is kept.
                // Convert the enum to a string to store as player prefs can only store float, int, and strings.
                PlayerPrefs.SetInt(_type.ToString(), 1);
                gameObject.SetActive(false);
            }
        }
    }
}
