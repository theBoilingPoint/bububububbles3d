using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class LevelsManager : MonoBehaviour
{
    public static LevelsManager Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private GameObject skillsMenu;
    [SerializeField] private TextMeshProUGUI text;

    [Header("Levels Setting")]
    [SerializeField] private int targetLevel = 3;
    [SerializeField] private int currentLevel = 0;

    private bool subscribed = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        TrySubscribeOrQueue();
    }

    private void OnDisable()
    {
        TryUnsubscribe();
    }

    private void TrySubscribeOrQueue()
    {
        // If ProgressBarFill is alive, (re)subscribe now
        if (ProgressBarFill.Instance != null && !subscribed)
        {
            ProgressBarFill.Instance.OnFilled += HandleProgressFilled;
            subscribed = true;
            return;
        }
        // Otherwise, wait one frame and try again (handles init order across scenes)
        if (!subscribed) StartCoroutine(SubscribeWhenReady());
    }

    private System.Collections.IEnumerator SubscribeWhenReady()
    {
        while (ProgressBarFill.Instance.IsUnityNull())
        {
            yield return null;
        }

        if (!subscribed && !ProgressBarFill.Instance.IsUnityNull())
        {
            ProgressBarFill.Instance.OnFilled += HandleProgressFilled;
            subscribed = true;
        }
    }

    private void TryUnsubscribe()
    {
        if (subscribed && ProgressBarFill.Instance !=null)
        {
            ProgressBarFill.Instance.OnFilled -= HandleProgressFilled;
        }
        subscribed = false;
    }

    private void Update()
    {
        currentLevel = Mathf.Min(currentLevel, targetLevel);
        if (!text.IsUnityNull())
        {
            text.text = $"Level: {currentLevel} / {targetLevel}";
        }
    }

    private void HandleProgressFilled()
    {
        if (currentLevel < targetLevel)
        {
            currentLevel += 1;
            Time.timeScale = 0f;
            skillsMenu.SetActive(true);
            ProgressBarFill.Instance.ResetFill();
        }
    }

    public bool ReachedTargetLevel() => currentLevel == targetLevel;

    public void ResetLevels(int startAt = 0)
    {
        currentLevel = Mathf.Clamp(startAt, 0, targetLevel);
        if (!ProgressBarFill.Instance.IsUnityNull())
        {
            ProgressBarFill.Instance.ResetFill();
        }
    }
}
