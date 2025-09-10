using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Skills
{
    Automation,
    Echo,
    TimeMaster
}

public class SkillsManager : MonoBehaviour
{
    public static SkillsManager Instance { get; private set; }

    [SerializeField] private GameObject skillsMenu;
    
    private Dictionary<Skills, int> skillsStackMap = new Dictionary<Skills, int>();
    
    private void Awake()
    {
        skillsStackMap.Clear();
        
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    public void ActivateAutomation()
    {
        UpdateSkillsStackMap(Skills.Automation);
        Time.timeScale = 1f;
        skillsMenu.SetActive(false);
    }

    public void ActivateEcho()
    {
        UpdateSkillsStackMap(Skills.Echo);
        Time.timeScale = 1f;
        skillsMenu.SetActive(false);
    }

    public void ActivateTimeMaster()
    {
        UpdateSkillsStackMap(Skills.TimeMaster);
        Time.timeScale = 1f;
        skillsMenu.SetActive(false);
    }

    private void UpdateSkillsStackMap(Skills skill)
    {
        if (skillsStackMap.ContainsKey(skill))
        {
            skillsStackMap[skill]++;
        }
        else
        {
            skillsStackMap[skill] = 1;
        }
    }
}
