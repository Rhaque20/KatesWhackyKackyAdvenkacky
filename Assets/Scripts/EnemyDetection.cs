using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    // layermasks uses the bitmask aka powers of 2 as such this rather arbitrary value was done
    // via math to get the magic number. More explained on Trigger enter function.
    const int GROUNDPLAYERMASK = 192;
    EnemyCore _enemyCore;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 192 = 2^6 + 2^7
        _enemyCore = GetComponentInParent<EnemyCore>();
    }

    /// <summary>
    /// When a collider enters the detection range, check if it's the player then do a vision check 
    /// to make sure it can actually see the player as its a sphere collider meaning it would be possible
    /// to see through walls.
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            // Perform a raycast from the current position to the player position to see
            // if there's a clear line of sight between the enemy and player
            RaycastHit2D[] results = Physics2D.RaycastAll(transform.position, collision.transform.position - transform.position, 
                Vector2.Distance(transform.position, collision.transform.position), 192);
            // If the raycast actually hit something
            if (results.Length > 0)
            {
                // Check if the first result is the player, if so, then pursue.
                if (results[0].collider.CompareTag("Player"))
                {
                    GetComponent<CircleCollider2D>().enabled = false;
                    _enemyCore.AlertEnemy(collision.GetComponent<KateCore>());
                }
            }
            
        }
    }


}
