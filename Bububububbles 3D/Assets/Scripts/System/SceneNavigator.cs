using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    public static SceneNavigator Instance { get; private set; }
    
    private bool subscribed = false;
    private bool _busy = false;
    
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
            if (!SkillsBinder.Instance.IsUnityNull()) SkillsBinder.Instance.ResetSkillSlots();
            if (!DynamicMenu.Instance.IsUnityNull()) DynamicMenu.Instance.ResetDynamicMenu();
        }

        SceneManager.LoadScene(sceneName);
    }
    
    public void GoToGameplayAfterSfx(string sfxName)
    {
        if (!_busy) StartCoroutine(Co_GoToSceneAfterSfx(sfxName, "GameplayScene"));
    }
    
    private IEnumerator Co_GoToSceneAfterSfx(string sfxName, string sceneName)
    {
        _busy = true;

        // kick the sound and wait
        if (AudioManager.Instance != null)
            yield return AudioManager.Instance.PlayClipAndWait(sfxName);

        // then run your existing reset + load logic
        GoToScene(sceneName);

        _busy = false;
    }

    public void QuitAfterSfx(string sfxName)
    {
        if (!_busy) StartCoroutine(Co_QuitAfterSfx(sfxName));
    }

    private IEnumerator Co_QuitAfterSfx(string sfxName)
    {
        _busy = true;

        if (AudioManager.Instance != null)
            yield return AudioManager.Instance.PlayClipAndWait(sfxName);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        _busy = false;
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
