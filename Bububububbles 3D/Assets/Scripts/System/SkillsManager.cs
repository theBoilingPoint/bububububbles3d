using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public enum Skills
{
    Automation,
    Echo,
    TimeMaster
}

public class SkillsManager : MonoBehaviour
{
    public static SkillsManager Instance { get; private set; }
    
    [Header("Prefabs")]
    [SerializeField] private GameObject skillsMenu;
    [SerializeField] private SkillSlot[] skillSlots;
    [SerializeField] private SkillScriptable[] skillScriptables;
    
    private Dictionary<Skills, int> skillsStackMap = new Dictionary<Skills, int>();
    private Dictionary<Skills, SkillScriptable> skillsScriptableMap = new Dictionary<Skills, SkillScriptable>();
    // note that the size of validKeyBindings should be at least that of SkillSlot
    private Queue<Key> validKeyBindings = new Queue<Key>();
    
    private void Awake()
    {
        validKeyBindings.Enqueue(Key.U);
        validKeyBindings.Enqueue(Key.I);
        validKeyBindings.Enqueue(Key.O);
        
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
            string skillname = skillScriptables[i].skillName;
            switch (skillname)
            {
                case "Automation":
                    skillsScriptableMap[Skills.Automation] = skillScriptables[i];
                    break;
                case "Echo":
                    skillsScriptableMap[Skills.Echo] = skillScriptables[i];
                    break;
                case "Time Master":
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
            Debug.LogError(skillsScriptableMap[s].skillName + " is not in the list of skills");
            return;
        }
        
        bool alreadyAdded = skillsStackMap.ContainsKey(s) &&  skillsStackMap[s] > 1;
        SkillScriptable newSkill = skillsScriptableMap[s];
        
        for (int i = 0; i < skillSlots.Length; i++)
        {
            SkillSlot slot = skillSlots[i];
            SkillScriptable skill = slot.GetSkillScriptable();
            if (alreadyAdded)
            {
                if (skill.skillName == skillsScriptableMap[s].skillName)
                {
                    slot.StackSkill(skillsStackMap[s]);
                    return;
                }
            }
            else {
                if (skill == null)
                {
                    bool isActive = isActiveSkill(s);
                    if (isActive)
                    {
                        slot.InitializeSkill(newSkill, validKeyBindings.Dequeue());
                    }
                    else
                    {
                        slot.InitializeSkill(newSkill);
                    }
                    
                    slot.gameObject.SetActive(true);
                    return;
                }
            }
            
        }
    }
    
    public void BindAutomation()
    {
        UpdateSkillsStackMap(Skills.Automation);
        AddSkillToSlot(Skills.Automation);
        Time.timeScale = 1f;
        skillsMenu.GetComponent<SkillsSelectionMenu>().ResetSkillsSelectionMenu();
        skillsMenu.SetActive(false);
    }

    public void BindEcho()
    {
        UpdateSkillsStackMap(Skills.Echo);
        AddSkillToSlot(Skills.Echo);
        Time.timeScale = 1f;
        skillsMenu.GetComponent<SkillsSelectionMenu>().ResetSkillsSelectionMenu();
        skillsMenu.SetActive(false);
    }

    public void BindTimeMaster()
    {
        UpdateSkillsStackMap(Skills.TimeMaster);
        AddSkillToSlot(Skills.TimeMaster);
        Time.timeScale = 1f;
        skillsMenu.GetComponent<SkillsSelectionMenu>().ResetSkillsSelectionMenu();
        skillsMenu.SetActive(false);
    }

    public void ResetSkillSlots()
    {
        for (int i = 0; i < skillSlots.Length; i++)
        {
            SkillSlot slot = skillSlots[i];
            slot.SetSkillScriptable(null);
            slot.gameObject.SetActive(false);
        }
        
        skillsStackMap.Clear();
    }

    private bool isActiveSkill(Skills s)
    {
        switch (s)
        {
            case Skills.Automation:
                return false;
            case Skills.Echo:
            case Skills.TimeMaster:
                return true;
            default:
                return false;
        }
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
