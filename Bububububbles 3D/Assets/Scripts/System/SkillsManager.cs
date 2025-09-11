using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEditor;
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
    [SerializeField] private SkillSlot[] skills;
    [SerializeField] private SkillScriptable[] skillScriptables;
    
    private Dictionary<Skills, int> skillsStackMap = new Dictionary<Skills, int>();
    private Dictionary<Skills, SkillScriptable> skillsScriptableMap = new Dictionary<Skills, SkillScriptable>();
    
    private void Awake()
    {
        skillsStackMap.Clear();
        
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeSkillsScriptableMap();
    }

    private void InitializeSkillsScriptableMap()
    {
        for (int i = 0; i < skillScriptables.Length; i++)
        {
            string skillname = skillScriptables[i].name;
            switch (skillname)
            {
                case "Automation":
                    skillsScriptableMap[Skills.Automation] = skillScriptables[i];
                    break;
                case "Echo":
                    skillsScriptableMap[Skills.Echo] = skillScriptables[i];
                    break;
                case "TimeMaster":
                    skillsScriptableMap[Skills.TimeMaster] = skillScriptables[i];
                    break;
                default:
                    Debug.LogError("The skill " +  skillname + " doesn't have a in code representation. Did you forget to add it to Skills struct?");
                    return;
            }
        }
    }
    
    public void AddSkillToSlot(Skills s)
    {
        if (!skillsScriptableMap.ContainsKey(s))
        {
            Debug.LogError(skillsScriptableMap[s].name + " is not in the list of skills");
            return;
        }

        SkillScriptable newSkill = skillsScriptableMap[s];
        for (int i = 0; i < skills.Length; i++)
        {
            SkillSlot slot = skills[i];
            SkillScriptable skill = slot.GetSkillScriptable();
            if (skill == null)
            {
                slot.InitializeSkill(newSkill);
                slot.gameObject.SetActive(true);
                return;
            }
        }
    }
    
    public void ActivateAutomation()
    {
        AddSkillToSlot(Skills.Automation);
        UpdateSkillsStackMap(Skills.Automation);
        Time.timeScale = 1f;
        skillsMenu.SetActive(false);
    }

    public void ActivateEcho()
    {
        AddSkillToSlot(Skills.Echo);
        UpdateSkillsStackMap(Skills.Echo);
        Time.timeScale = 1f;
        skillsMenu.SetActive(false);
    }

    public void ActivateTimeMaster()
    {
        AddSkillToSlot(Skills.TimeMaster);
        UpdateSkillsStackMap(Skills.TimeMaster);
        Time.timeScale = 1f;
        skillsMenu.SetActive(false);
    }

    public void ResetSkillSlots()
    {
        for (int i = 0; i < skills.Length; i++)
        {
            SkillSlot slot = skills[i];
            slot.SetSkillScriptable(null);
            slot.gameObject.SetActive(false);
        }
        
        skillsStackMap.Clear();
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
