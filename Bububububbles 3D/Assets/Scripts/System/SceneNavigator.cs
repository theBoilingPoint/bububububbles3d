using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    private void Update()
    {
        ManageInGameSceneTransition();
    }

    public void GoToScene(string sceneName)
    {
        Time.timeScale = 1f;

        if (sceneName == "GameplayScene")
        {
            if (Timer.Instance != null) Timer.Instance.ResetTime();
            if (LevelsManager.Instance != null) LevelsManager.Instance.ResetLevels(0);
            if (ProgressBarFill.Instance != null) ProgressBarFill.Instance.ResetFill();
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

    private void ResetGameplay()
    {
        if (Timer.Instance.IsUnityNull() || 
            ProgressBarFill.Instance.IsUnityNull() ||
            LevelsManager.Instance.IsUnityNull())
        {
            throw new System.Exception("Cannot reset level.");
        }
        
        Timer.Instance.ResetTime();
        ProgressBarFill.Instance.ResetFill();
        LevelsManager.Instance.ResetLevels(0);
    }
    
    private void ManageInGameSceneTransition()
    {
        if (SceneManager.GetActiveScene().name != "GameplayScene")
        {
            return;
        }
        
        if (Timer.Instance.GetTime() > 0.0f)
        {
            if (LevelsManager.Instance.ReachedTargetLevel())
            {
                ResetGameplay();
                SceneManager.LoadScene("WinScene");
            }
        }
        else
        {
            ResetGameplay();
            SceneManager.LoadScene("LoseScene");
        }
    }
}
