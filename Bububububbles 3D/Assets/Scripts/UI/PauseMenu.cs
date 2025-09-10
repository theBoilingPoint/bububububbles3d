using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
  [SerializeField] private GameObject pauseMenu;
  [SerializeField] private GameObject skillsMenu;
  
  public static bool isPaused = false;

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
}
