using UnityEngine;

public class StartMenu : MonoBehaviour
{
    [SerializeField] private GameObject rulesPanel;

    private void Start()
    {
        rulesPanel.SetActive(false);
    }

    public void toggleRulesPanel()
    {
        if (rulesPanel.activeSelf)
        {
            rulesPanel.SetActive(false);
        }
        else
        {
            rulesPanel.SetActive(true);
        }
    }
}
