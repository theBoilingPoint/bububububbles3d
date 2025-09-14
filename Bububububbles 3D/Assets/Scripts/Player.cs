using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private BubblesManager bubblesManager;
    
    public static Player Instance { get; private set; }
    
    [HideInInspector] public bool hasCollidedWithBubbles = false;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        hasCollidedWithBubbles = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;
        BubblesManager mgr = FindObjectOfType<BubblesManager>();
        if (mgr == null)
        {
            Debug.LogError("BubblesManager is not found!");
            return;
        }
        
        if (other.CompareTag(Bubble.NormalBubble.ToString()))
        {
            ProgressBarFill.Instance.UpdateCurrentScore(bubblesManager.bubbleScoreMap[Bubble.NormalBubble]);
            mgr.RemoveBubble(other.gameObject);
            Destroy(other.gameObject);
            hasCollidedWithBubbles = true;
        }

        if (other.CompareTag(Bubble.AddTimeBubble.ToString()))
        {
            Timer.Instance.AddTime(bubblesManager.bubbleScoreMap[Bubble.AddTimeBubble]);
            mgr.RemoveBubble(other.gameObject);
            Destroy(other.gameObject);
            hasCollidedWithBubbles = true;
        }
        
        if (other.CompareTag(Bubble.DangerBubble.ToString()))
        {
            ProgressBarFill.Instance.UpdateCurrentScore(bubblesManager.bubbleScoreMap[Bubble.DangerBubble]);
            mgr.RemoveBubble(other.gameObject);
            Destroy(other.gameObject);
            hasCollidedWithBubbles = true;
        }
    }
}
