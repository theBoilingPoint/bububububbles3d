using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public enum Skill
{
    Automation,
    Echo,
    TimeMaster
}

public class SkillsBinder : MonoBehaviour
{
    public static SkillsBinder Instance { get; private set; }
    
    [Header("Prefabs")]
    [SerializeField] private GameObject skillsMenu;
    [SerializeField] private SkillSlot[] skillSlots;
    [SerializeField] private SkillScriptable[] skillScriptables;
    
    public Dictionary<Skill, int> skillsStackMap = new Dictionary<Skill, int>();
    private Dictionary<Skill, SkillScriptable> skillsScriptableMap = new Dictionary<Skill, SkillScriptable>();
    // note that the size of validKeyBindings should be at least that of SkillSlot
    private Queue<Key> validKeyBindings = new Queue<Key>();
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeSkillsScriptableMap();
        InitializeValidKeyBindings();
    }

    private void InitializeSkillsScriptableMap()
    {
        for (int i = 0; i < skillScriptables.Length; i++)
        {
            Skill skill = skillScriptables[i].skill;
            switch (skill)
            {
                case Skill.Automation:
                    skillsScriptableMap[Skill.Automation] = skillScriptables[i];
                    break;
                case Skill.Echo:
                    skillsScriptableMap[Skill.Echo] = skillScriptables[i];
                    break;
                case Skill.TimeMaster:
                    skillsScriptableMap[Skill.TimeMaster] = skillScriptables[i];
                    break;
                default:
                    Debug.LogError("The skill " +  skill + " doesn't have a in code representation. Did you forget to add it to Skills struct?");
                    return;
            }
        }
    }

    private void InitializeValidKeyBindings()
    {
        validKeyBindings.Enqueue(Key.U);
        validKeyBindings.Enqueue(Key.I);
        validKeyBindings.Enqueue(Key.O);
    }
    
    public void AddSkillToSlot(Skill s)
    {
        if (!skillsScriptableMap.ContainsKey(s))
        {
            Debug.LogError(skillsScriptableMap[s].skill + " is not in the list of skills");
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
                if (skill.skill == skillsScriptableMap[s].skill)
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
        UpdateSkillsStackMap(Skill.Automation);
        AddSkillToSlot(Skill.Automation);
        Time.timeScale = 1f;
        skillsMenu.GetComponent<SkillsSelectionMenu>().ResetSkillsSelectionMenu();
        skillsMenu.SetActive(false);
    }

    public void BindEcho()
    {
        UpdateSkillsStackMap(Skill.Echo);
        AddSkillToSlot(Skill.Echo);
        Time.timeScale = 1f;
        skillsMenu.GetComponent<SkillsSelectionMenu>().ResetSkillsSelectionMenu();
        skillsMenu.SetActive(false);
    }

    public void BindTimeMaster()
    {
        UpdateSkillsStackMap(Skill.TimeMaster);
        AddSkillToSlot(Skill.TimeMaster);
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

    private bool isActiveSkill(Skill s)
    {
        switch (s)
        {
            case Skill.Automation:
                return false;
            case Skill.Echo:
            case Skill.TimeMaster:
                return true;
            default:
                return false;
        }
    }

    private void UpdateSkillsStackMap(Skill skill)
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
