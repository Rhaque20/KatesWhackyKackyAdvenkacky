using System.Collections;
using TMPro;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] Transform _wavesData, _enemyWaves, _obstacles;
    [SerializeField] GameObject _waveTrackerUI;
    const string NEXTWAVETEXT = "Next Wave In: ", ACTIVEWAVE = "Current Wave: ";
    int _currentRound = 0, _remainingEnemies = 0;
    TMP_Text _enemyCounter, _waveDelayTimeText, _currentWaveDisplay;
    Coroutine _activeWaveCountDownTimer = null;


    private void Start()
    {
        _enemyCounter = _wavesData.GetChild(0).GetComponent<TMP_Text>();
        _currentWaveDisplay = _waveTrackerUI.GetComponent<TMP_Text>();
        _waveDelayTimeText = _waveTrackerUI.transform.GetChild(0).GetComponent<TMP_Text>();
        _enemyCounter.SetText("0");

        for (int i = 0; i < _enemyWaves.childCount; i++)
        {
            _enemyWaves.GetChild(i).gameObject.SetActive(false);
            _obstacles.GetChild(i).gameObject.SetActive(false);
        }
    }

    void StartWave()
    {
        _enemyWaves.GetChild(_currentRound).gameObject.SetActive(true);
        _obstacles.GetChild(_currentRound).gameObject.SetActive(true);
        LevelManager.Instance.BindEnemiesToCounter(_enemyWaves.GetChild(_currentRound));
        _currentRound++;
    }

    private IEnumerator WaveCountDownTimer()
    {
        float _countDown = 5f;
        _waveDelayTimeText.SetText(_countDown.ToString("F1"));
        _currentWaveDisplay.SetText(NEXTWAVETEXT);
        _remainingEnemies = _enemyWaves.GetChild(_currentRound).childCount;

        while (_countDown > 0f)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            _countDown -= Time.deltaTime;
            _waveDelayTimeText.SetText(_countDown.ToString("F1"));
        }
        _activeWaveCountDownTimer = null;
        _currentWaveDisplay.SetText(ACTIVEWAVE);
        string waveString = _currentRound + "/" + _enemyWaves.childCount;
        _waveDelayTimeText.SetText(waveString);
        _enemyCounter.SetText(_remainingEnemies.ToString());
        StartWave();
        
    }

    public void DecrementCounter()
    {
        _remainingEnemies--;
        _enemyCounter.SetText(_remainingEnemies.ToString());
        if(_remainingEnemies < 1)
        {
            if (_currentRound >= _enemyWaves.childCount)
                LevelManager.Instance.EnterNextLevel();
            else
                StartCountDown();
        }
    }

    public void StartCountDown()
    {
        if (_waveDelayTimeText == null)
        {
            Start();
        }
        if(_activeWaveCountDownTimer == null)
        {
            _activeWaveCountDownTimer = StartCoroutine(WaveCountDownTimer());
        }
    }
}
