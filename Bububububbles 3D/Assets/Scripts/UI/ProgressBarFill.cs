using System;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarFill : MonoBehaviour
{
    public static ProgressBarFill Instance { get; private set; }
    
    [SerializeField] private Image progressBarFill;
    [SerializeField] private float max = 100f;
    [SerializeField] private float current = 0.0f;
    
    public event Action OnFilled;
    private bool filledLatch = false;
    
    private void Awake()
    {
        progressBarFill.fillAmount = 0;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        UpdateFill();
    }

    public void UpdateCurrentScore(float score)
    {
        current += score;
    }

    public void ResetFill()
    {
        progressBarFill.fillAmount = 0;
        current = 0;
        filledLatch = false;   
    }

    void UpdateFill()
    {
        current = Mathf.Max(0f, current);
        progressBarFill.fillAmount = current / max;

        bool filledNow = progressBarFill.fillAmount >= 1f;

        // Rising edge: fires once
        if (filledNow && !filledLatch)
        {
            filledLatch = true;
            OnFilled?.Invoke();
        }
        // Drop the latch if it’s no longer full
        else if (!filledNow && filledLatch)
        {
            filledLatch = false;
        }
    }
}
