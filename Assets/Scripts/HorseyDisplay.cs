using UnityEngine;
using UnityEngine.UI;

public class HorseyDisplay : MonoBehaviour
{
    const int TURKEYLEGOFFSET = 1;
    // Variables to set up the display
    [SerializeField]Transform _horseyTransform;
    [SerializeField] GameObject _horseyVictoryScreen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    /// <summary>
    /// Initialize the necessary values then evaluates on the player 
    /// based on how many secret unicorns they have collected.
    /// </summary>
    void Start()
    {
        bool _hasHorsey = false;
        int horseyCount = 0;
        Collectable.CollectibleType _typing;
        for (int i = 0; i < _horseyTransform.childCount; i++)
        {
            _typing = (Collectable.CollectibleType)i + TURKEYLEGOFFSET;// First converts the int into the enum with 1 offset as 0 is turkey leg.
            _hasHorsey = PlayerPrefs.HasKey(_typing.ToString());// Then uses the enum to convert to string to see if the player has collected that horsey.
            Debug.Log("Checking on horsey type" +_typing);
            if(_hasHorsey )
            {
                horseyCount++;
                Debug.Log("Has the horsey");
            }
             // Set the horsey to visible to indicate the horseys found in the order of the level they're supposed to be found.
            _horseyTransform.GetChild(i).GetComponent<Image>().color = (_hasHorsey ? Color.white : Color.black);
        }

        if(horseyCount == 3)// If they have all three horseys, set up the special victory screen.
        {
            _horseyVictoryScreen.SetActive(true);
        }
    }
}
