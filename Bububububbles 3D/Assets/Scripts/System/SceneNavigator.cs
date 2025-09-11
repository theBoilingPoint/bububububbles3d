using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    public static SceneNavigator Instance { get; private set; }
    
    private bool subscribed = false;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
        if (!subscribed &&
            !LevelsManager.Instance.IsUnityNull() &&
            !Timer.Instance.IsUnityNull())
        {
            LevelsManager.Instance.OnTargetLevelReached += HandleWin;
            Timer.Instance.onTimeOut += HandleLose;
            subscribed = true;
            return;
        }
        // Otherwise, wait one frame and try again (handles init order across scenes)
        if (!subscribed) StartCoroutine(SubscribeWhenReady());
    }

    private IEnumerator SubscribeWhenReady()
    {
        while (LevelsManager.Instance.IsUnityNull() ||
               Timer.Instance.IsUnityNull())
        {
            yield return null;
        }

        if (!subscribed && 
            !LevelsManager.Instance.IsUnityNull()
            && !Timer.Instance.IsUnityNull())
        {
            LevelsManager.Instance.OnTargetLevelReached += HandleWin;
            Timer.Instance.onTimeOut += HandleLose;
            subscribed = true;
        }
    }

    private void TryUnsubscribe()
    {
        if (subscribed)
        {
            if (!LevelsManager.Instance.IsUnityNull()) LevelsManager.Instance.OnTargetLevelReached -= HandleWin;
            
        }
        subscribed = false;
    }

    public void GoToScene(string sceneName)
    {
        Time.timeScale = 1f;
 
        if (sceneName == "GameplayScene")
        {
            if (!Timer.Instance.IsUnityNull()) Timer.Instance.ResetTime();
            if (!LevelsManager.Instance.IsUnityNull()) LevelsManager.Instance.ResetLevels(0);
            if (!ProgressBarFill.Instance.IsUnityNull()) ProgressBarFill.Instance.ResetFill();
        }
        else
        {
            if (!SkillsManager.Instance.IsUnityNull()) SkillsManager.Instance.ResetSkillSlots();
            if (!DynamicMenu.Instance.IsUnityNull()) DynamicMenu.Instance.ResetDynamicMenu();
        }

        SceneManager.LoadScene(sceneName);
    }
    
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

    private void HandleWin()
    {
        if (SceneManager.GetActiveScene().name != "GameplayScene")
        {
            return;
        }
        
        GoToScene("WinScene");
    }

    private void HandleLose()
    {
        if (SceneManager.GetActiveScene().name != "GameplayScene")
        {
            return;
        }
        
        GoToScene("LoseScene");
    }
}
