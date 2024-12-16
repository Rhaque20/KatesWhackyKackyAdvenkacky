using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class LevelManager : MonoBehaviour
{
    // Handle different logic based on the level the player is in.
    public enum LevelCategory { Level1,Level2,Level3}
    // Main menu index number
    const int MAIN = 0;
    
    [SerializeField] LevelCategory _currentLevelManagement;
    [SerializeField] BoxCollider2D _nextSceneTrigger;
    // Used for handling what occurs when the checkpoint has been entered
    [SerializeField] Transform _effectQueueRef;
    // Used for when the checkpoint ordeal has been cleared (opening door, starting next wave, etc.)
    [SerializeField] Transform _onEffectEndQueueRef;

    [SerializeField] GameObject _gameOverScreen,_pauseScreen;
    // Sadly unused, meant to be funny lines on pause.
    [SerializeField] string[] _funnyLines = new string[3];

    // List of all transforms of possible effects that will be removed after it's been processed.
    List<Transform> _effectQueue = new List<Transform>();
    // Manager singleton.
    public static LevelManager Instance;
    // Used to handle all the necessary logic before changing scenes.
    public Action sceneChangeFunctions;
    // Important for level 3 to handle wave logic
    private WaveManager _waveManager;

    private bool _waitingForWaveToFinish = false;
    private int _enemyCount = 0;
    private int _effectIndex = 0;

    private TMP_Text _pauseMenuText;

    /// <summary>
    ///  Called before all start functions to set up singleton.
    /// </summary>
    private void Awake()
    {
        // If a singleton instance exists and it's not this, destroy itself.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    /// <summary>
    /// Sets up the levelmanager with all the necessary effects as well as pause menu and game over screen,
    /// </summary>
    private void Start()
    {
        Debug.Log("Start has been called in level manager");
        if(_effectQueueRef != null)
        {
            for (int i = 0; i < _effectQueueRef.childCount; i++)
            {
                _effectQueue.Add(_effectQueueRef.GetChild(i));
                _effectQueue[i].gameObject.SetActive(false);
            }
        }

        if(_gameOverScreen != null)
        {
            _gameOverScreen.SetActive(false);
        }
        if(_pauseScreen != null)
        {
            _pauseScreen.SetActive(false);
            _pauseMenuText = _pauseScreen.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>();
        }

        if (_currentLevelManagement == LevelCategory.Level3)
        {
            _waveManager = GetComponent<WaveManager>();
            if (_waveManager != null)
            {
                _waveManager.StartCountDown();
            }
        }


    }

    /// <summary>
    /// Trigger any effect when entering a checkpoint
    /// </summary>
    /// <param name="effect">Effect type to trigger</param>
    public void TriggerAtCheckpoint(Checkpoint.TriggerEffect effect)
    {
        if(effect != Checkpoint.TriggerEffect.None && _effectQueue.Count > 0)
        {
            _effectQueue[0].gameObject.SetActive(true);
            if (effect == Checkpoint.TriggerEffect.SpawnEnemyWave)
            {
                Debug.Log("Binding enemies to upcoming wave");
                _waitingForWaveToFinish = true;
                BindEnemiesToCounter(_effectQueue[0]);
            }
            _effectQueue.RemoveAt(0);
            
        }
        
    }

    /// <summary>
    /// Decrease the current wave counter and do something when wave is claered.
    /// </summary>
    public void DecrementWaveCounter()
    {
        // In case of human error and binded a death function to wave decrement.
        if(_waitingForWaveToFinish)
        {
            Debug.Log("Decremented");
            _enemyCount--;
            if(_waveManager != null)
                _waveManager.DecrementCounter();// Decrease wave manager counter.

            if(_enemyCount < 1)// If all enemies are cleared do the effect.
            {
                _waitingForWaveToFinish = false;
                if(_onEffectEndQueueRef != null)
                {
                    bool effectActiveSelf = _onEffectEndQueueRef.GetChild(_effectIndex).gameObject.activeSelf;
                    _onEffectEndQueueRef.GetChild(_effectIndex).gameObject.SetActive(!effectActiveSelf);
                    _effectIndex++;
                }
            }
        }
    }

    /// <summary>
    /// Binds the enemies of the transform to this so that when they die, it's communicated to here
    /// </summary>
    /// <param name="enemyList">Transform containing all the enemy children</param>
    public void BindEnemiesToCounter(Transform enemyList)
    {
        // Only matters for when being called by wave manager.
        if (!_waitingForWaveToFinish)
            _waitingForWaveToFinish = true;

        for (int i = 0; i < enemyList.childCount; i++)
        {
            Debug.Log("Binding "+ enemyList.GetChild(i).name);
            enemyList.GetChild(i).GetComponent<EnemyCore>().BindTargetToWaveCounter();
            _enemyCount++;
        }
    }

    /// <summary>
    /// Used for transitioning to next level.
    /// </summary>
    public void EnterNextLevel()
    {
        sceneChangeFunctions?.Invoke();
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    /// <summary>
    /// Used to start transitioning to next level
    /// </summary>
    /// <param name="collision">Hopefully player collider</param>
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
            EnterNextLevel();
    }

    /// <summary>
    /// Pull up game over screen.
    /// </summary>
    public void GameOver()
    {
        if(_gameOverScreen != null)
            _gameOverScreen.SetActive(true);
    }

    /// <summary>
    /// Reset the level and set the timescale back to normal if called from pause menu.
    /// </summary>
    public void ResetLevel()
    {
        Time.timeScale = 1.0f;
        sceneChangeFunctions?.Invoke();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    ///  Used at main menu to reset the horse progress.
    /// </summary>
    public void ResetProgress()
    {
        int n = Enum.GetNames(typeof(Collectable.CollectibleType)).Length;
        Collectable.CollectibleType type;
        for(int i = 0; i < n; i++)
        {
            type = (Collectable.CollectibleType)i;
            if(PlayerPrefs.HasKey(type.ToString()))
            {
                PlayerPrefs.DeleteKey((type.ToString()));
            }
        }
    }

    /// <summary>
    /// Return to main menu
    /// </summary>
    public void ReturnToMain()
    {
        sceneChangeFunctions?.Invoke();
        SceneManager.LoadScene(MAIN);
    }
    /// <summary>
    /// Pause the game. Works flip flop so if called while game already paused, resume.
    /// </summary>
    public void PauseMenu()
    {
        if(_pauseScreen != null)
        {
            if(Time.timeScale < 0.1f)
                ResumeGame();
            else
            {
                Time.timeScale = 0.0f;
                _pauseScreen.SetActive(true);
                _pauseMenuText.SetText(_funnyLines[Random.Range(0,3)]);
            }
            
        }
        
    }

    /// <summary>
    /// Used to quit the game.
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }

    /// <summary>
    ///  Resume the game.
    /// </summary>
    public void ResumeGame()
    {
        _pauseScreen.SetActive(false);
        Time.timeScale = 1.0f;
    }

    /// <summary>
    /// Too lazy to make input action. Just calls Pause Menu
    /// </summary>
    public void Update()
    {
        if(Input.GetKeyUp("escape"))
        {
            PauseMenu();
        }
    }
}
