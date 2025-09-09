using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarFill : MonoBehaviour
{
    public static ProgressBarFill Instance { get; private set; }
    
    [SerializeField] private Image progressBarFill;
    [SerializeField] private float max = 100f;
    [SerializeField] private float current = 0.0f;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        UpdateFill();
    }

    public void UpdateCurrentScore(float score)
    {
        current += score;
    }

    public bool isFilled()
    {
        float progress = Mathf.Min(1f, current / max);
        return progress >= 1f;
    }

    public void ResetFill()
    {
        progressBarFill.fillAmount = 0;
    }

    void UpdateFill()
    {
        current = Mathf.Max(0.0f, current);
        progressBarFill.fillAmount = current / max;
    }
}
