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

    private void ResetLevel()
    {
        if (Timer.Instance.IsUnityNull() || ProgressBarFill.Instance.IsUnityNull())
        {
            return;
        }
        
        Timer.Instance.ResetTime();
        ProgressBarFill.Instance.ResetFill();
    }
    
    private void ManageInGameSceneTransition()
    {
        if (SceneManager.GetActiveScene().name != "GameplayScene")
        {
            return;
        }
        
        if (Timer.Instance.GetTime() > 0.0f)
        {
            if (ProgressBarFill.Instance.isFilled())
            {
                ResetLevel();
                SceneManager.LoadScene("WinScene");
            }
        }
        else
        {
            ResetLevel();
            SceneManager.LoadScene("LoseScene");
        }
    }
}
