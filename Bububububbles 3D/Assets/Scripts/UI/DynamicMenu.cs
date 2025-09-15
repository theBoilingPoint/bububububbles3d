using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class DynamicMenu : MonoBehaviour
{
  public static DynamicMenu Instance { get; private set; }

  [Header("Prefabs")]
  [SerializeField] private GameObject pauseMenu;
  [SerializeField] private GameObject skillsMenu;
  [SerializeField] private GameObject pauseFirstSelected;   // e.g., Resume button
  [SerializeField] private GameObject skillsFirstSelected;  // e.g., first skill slot

  [Header("Input (PlayerInput + map names)")]
  [SerializeField] private PlayerInput playerInput;     // ← assign the Player GameObject (has PlayerInput)
  [SerializeField] private string gameplayMap = "Player";
  [SerializeField] private string uiMap = "UI";

  public static bool isPaused = false;
  public static bool IsPauseMenuOpen =>
    Instance != null && Instance.pauseMenu != null && Instance.pauseMenu.activeSelf;
  public static bool IsSkillsMenuOpen =>
    Instance != null && Instance.skillsMenu != null && Instance.skillsMenu.activeSelf;
  public static bool IsAnyMenuOpen => IsPauseMenuOpen || IsSkillsMenuOpen;
  
  private bool subscribed = false;

  // Actions we hook into (from the asset)
  private InputAction pauseAction;   // Gameplay/Pause (Esc or Start)
  private InputAction cancelAction;  // UI/Cancel   (Esc) – we’ll toggle it per menu type

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
    if (playerInput != null && playerInput.actions != null)
    {
      pauseAction  = playerInput.actions.FindAction("Pause", throwIfNotFound: true);
      cancelAction = playerInput.actions.FindAction("Cancel", throwIfNotFound: false);

      pauseAction.performed  += OnPausePerformed;
      pauseAction.Enable();

      if (cancelAction != null)
      {
        cancelAction.performed += OnCancelPerformed;
      }
    }

    TrySubscribeOrQueue();
  }

  private void OnDisable()
  {
    TryUnsubscribe();

    if (pauseAction != null)
    {
      pauseAction.performed -= OnPausePerformed;
      pauseAction = null;
    }

    if (cancelAction != null)
    {
      cancelAction.performed -= OnCancelPerformed;
      cancelAction = null;
    }
  }

  // Called when Esc is pressed in the UI map *and* cancelAction is enabled
  private void OnCancelPerformed(InputAction.CallbackContext ctx)
  {
    // Only close if pause menu is active (skills menu disables Esc)
    if (pauseMenu != null && pauseMenu.activeSelf)
    {
      ResumeGame();
    }
  }

  private void OnPausePerformed(InputAction.CallbackContext ctx)
  {
    // Ignore while the skills menu is active (Esc should do nothing there).
    if (skillsMenu != null && skillsMenu.activeSelf) return;

    if (isPaused) ResumeGame();
    else          PauseGame();
  }

  private void TrySubscribeOrQueue()
  {
    if (LevelsManager.Instance != null && !subscribed)
    {
      LevelsManager.Instance.OnLevelIncremented += HandleLevelIncremented;
      subscribed = true;
      return;
    }
    if (!subscribed) StartCoroutine(SubscribeWhenReady());
  }

  private IEnumerator SubscribeWhenReady()
  {
    while (LevelsManager.Instance.IsUnityNull())
      yield return null;

    if (!subscribed && !ProgressBarFill.Instance.IsUnityNull())
    {
      LevelsManager.Instance.OnLevelIncremented += HandleLevelIncremented;
      subscribed = true;
    }
  }

  private void TryUnsubscribe()
  {
    if (subscribed && LevelsManager.Instance != null)
      LevelsManager.Instance.OnLevelIncremented -= HandleLevelIncremented;
    subscribed = false;
  }

  // ——— MENU OPEN/CLOSE ——————————————————————————————————————
  // Triggered by your level-up flow
  private void HandleLevelIncremented()
  {
    Time.timeScale = 0f;
    SwitchToUI(allowEsc: false);
    skillsMenu.GetComponent<SkillsSelectionMenu>().InitializeSkillsSelectionMenu();
    skillsMenu.SetActive(true);

    if (skillsFirstSelected != null)
      EventSystem.current?.SetSelectedGameObject(skillsFirstSelected);
  }

  private void PauseGame()
  {
    if (skillsMenu != null && skillsMenu.activeSelf) return;
    pauseMenu.SetActive(true);
    Time.timeScale = 0f;
    isPaused = true;
    SwitchToUI(allowEsc: true);

    if (pauseFirstSelected != null)
      EventSystem.current?.SetSelectedGameObject(pauseFirstSelected);
  }

  public void ResumeGame()
  {
    if (skillsMenu != null && skillsMenu.activeSelf) return;
    pauseMenu.SetActive(false);
    Time.timeScale = 1f;
    isPaused = false;
    SwitchToGameplay();
    EventSystem.current?.SetSelectedGameObject(null); // clear selection
  }
  
  public void CloseSkillsMenu()
  {
    if (skillsMenu != null) skillsMenu.SetActive(false);

    // optional: clear out button listeners/text so they don't stack next time
    if (SkillsSelectionMenu.Instance != null)
      SkillsSelectionMenu.Instance.ResetSkillsSelectionMenu();

    Time.timeScale = 1f;
    isPaused = false;
    SwitchToGameplay();                           // <-- back to "Player" map
    EventSystem.current?.SetSelectedGameObject(null);
  }
  
  public void ResetDynamicMenu()
   {
     isPaused = false;
     subscribed = false;

     pauseMenu.SetActive(false);
     skillsMenu.SetActive(false);

     Time.timeScale = 1f;
   }

  // ——— MAP SWITCH HELPERS ————————————————————————————————————
  private void SwitchToUI(bool allowEsc)
  {
    if (playerInput == null) return;

    // Switch maps
    playerInput.SwitchCurrentActionMap(uiMap);

    // Toggle Esc availability *inside* the UI map
    if (cancelAction != null)
    {
      if (allowEsc) cancelAction.Enable();
      else          cancelAction.Disable();
    }
  }

  private void SwitchToGameplay()
  {
    if (playerInput == null) return;
    playerInput.SwitchCurrentActionMap(gameplayMap);
    // `Cancel` state doesn’t matter in Gameplay; it’s not in that map.
  }
}
