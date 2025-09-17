using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public static Timer Instance { get; private set; }
    
    [Header("Prefabs")]
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TextMeshProUGUI timerFreezeCountdownText;

    [Header("Params")]
    [SerializeField] private float defaultTime;
    [SerializeField] private Color timerColor = Color.white;
    [SerializeField] private Color timerFreezeColor = Color.blue;

    private bool isFrozen = false;
    private float time;
    private bool signaledTimeOut = false;

    private Coroutine freezeRoutine;   // track running freeze coroutine

    public event Action onTimeOut;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        time = defaultTime;
        if (text != null) text.color = timerColor;
        if (timerFreezeCountdownText != null)
        {
            timerFreezeCountdownText.color = timerFreezeColor;
            timerFreezeCountdownText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isFrozen)
        {
            if (time > 0f)
            {
                time -= Time.deltaTime;
                if (time < 0f) time = 0f;
            }
            else
            {
                if (!signaledTimeOut)
                {
                    onTimeOut?.Invoke();
                    signaledTimeOut = true;
                }
            }
        }

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        if (text != null)
            text.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void AddTime(float amount) => time += amount;

    public void ResetTime()
    {
        time = defaultTime;
        signaledTimeOut = false;
        ResetTimeFreeze();
    }
    
    public void FreezeTimer(float seconds)
    {
        if (!gameObject.activeInHierarchy) return;

        ResetTimeFreeze();
        freezeRoutine = StartCoroutine(FreezeRoutine(seconds));
    }

    private IEnumerator FreezeRoutine(float seconds)
    {
        isFrozen = true;
        if (text != null) text.color = timerFreezeColor;

        float remaining = seconds;
        if (timerFreezeCountdownText != null)
        {
            timerFreezeCountdownText.gameObject.SetActive(true);
        }

        while (remaining > 0f)
        {
            // update countdown display
            if (timerFreezeCountdownText != null)
            {
                timerFreezeCountdownText.text = Mathf.CeilToInt(remaining).ToString();
            }

            // wait one frame and subtract deltaTime
            yield return null;
            remaining -= Time.deltaTime;
        }

        // clean up when freeze ends
        isFrozen = false;
        if (text != null) text.color = timerColor;
        if (timerFreezeCountdownText != null)
        {
            timerFreezeCountdownText.gameObject.SetActive(false);
        }

        freezeRoutine = null;
    }

    private void ResetTimeFreeze()
    {
        // If a freeze routine is already running, stop it and reset color
        if (freezeRoutine != null)
        {
            StopCoroutine(freezeRoutine);
            freezeRoutine = null;
            if (!text.IsUnityNull()) text.color = timerColor;
        }
        
        isFrozen = false;

        timerFreezeCountdownText.text = 0.ToString();
        timerFreezeCountdownText.gameObject.SetActive(false);
    }
}
