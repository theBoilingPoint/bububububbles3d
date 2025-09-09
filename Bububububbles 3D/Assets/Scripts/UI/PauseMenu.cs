using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
  [SerializeField] private GameObject pauseMenu;
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
    pauseMenu.SetActive(true);
    Time.timeScale = 0;
    isPaused = true;
  }
  
  public void ResumeGame()
  {
    pauseMenu.SetActive(false);
    Time.timeScale = 1f;
    isPaused = false;
  }
}
