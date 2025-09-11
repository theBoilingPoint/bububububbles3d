using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DynamicMenu : MonoBehaviour
{
  public static DynamicMenu Instance { get; private set; }
  
  [SerializeField] private GameObject pauseMenu;
  [SerializeField] private GameObject skillsMenu;
  
  public static bool isPaused = false;
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

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.Escape))
    {
      if (isPaused)
      {
        ResumeGame();
      }
      else
      {
        PauseGame();
      }
    }
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
    if (LevelsManager.Instance != null && !subscribed)
    {
      LevelsManager.Instance.OnLevelIncremented += HandleLevelIncremented;
      subscribed = true;
      return;
    }
    // Otherwise, wait one frame and try again (handles init order across scenes)
    if (!subscribed) StartCoroutine(SubscribeWhenReady());
  }

  private IEnumerator SubscribeWhenReady()
  {
    while (LevelsManager.Instance.IsUnityNull())
    {
      yield return null;
    }

    if (!subscribed && !ProgressBarFill.Instance.IsUnityNull())
    {
      LevelsManager.Instance.OnLevelIncremented += HandleLevelIncremented;
      subscribed = true;
    }
  }
  
  private void TryUnsubscribe()
  {
    if (subscribed && LevelsManager.Instance !=null)
    {
      LevelsManager.Instance.OnLevelIncremented -= HandleLevelIncremented;
    }
    subscribed = false;
  }

  private void HandleLevelIncremented()
  {
    Time.timeScale = 0f;
    skillsMenu.GetComponent<SkillsSelectionMenu>().InitializeSkillsSelectionMenu();
    skillsMenu.SetActive(true);
  }

  private void PauseGame()
  {
    if (skillsMenu.activeSelf)
    {
      return;
    }
    
    pauseMenu.SetActive(true);
    Time.timeScale = 0;
    isPaused = true;
  }
  
  public void ResumeGame()
  {
    if (skillsMenu.activeSelf)
    {
      return;
    }
    
    pauseMenu.SetActive(false);
    Time.timeScale = 1f;
    isPaused = false;
  }

  public void ResetDynamicMenu()
  {
    isPaused = false;
    subscribed = false;
    
    pauseMenu.SetActive(false);
    skillsMenu.SetActive(false);
    
    Time.timeScale = 1f;
  }
}
