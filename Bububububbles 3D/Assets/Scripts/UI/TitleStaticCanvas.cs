using UnityEngine;
using UnityEngine.EventSystems;

public class TitleStaticCanvas : MonoBehaviour
{
    [SerializeField] private GameObject firstSelected;
    
    public static TitleStaticCanvas Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (firstSelected != null)
        {
            EventSystem.current?.SetSelectedGameObject(firstSelected);
        }
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Cursor.visible   = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
