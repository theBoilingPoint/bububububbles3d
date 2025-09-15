using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SkillsExecutor : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private BubblesManager bubblesManager;
    [SerializeField] private Player player;
    
    [Header("Params")]
    [SerializeField] private float automationInterval = 1f; // how frequently to execute automation in s

    [SerializeField] private float timeMasterDuration = 2f; // The duration of the time master effect
    
    public static SkillsExecutor Instance { get; private set; }
    
    // skills
    private Dictionary<Skill, bool> skillActivationMap = new Dictionary<Skill, bool>();
    private float automationTimer = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        skillActivationMap.Add(Skill.Automation, false);
        skillActivationMap.Add(Skill.Echo, false);
    }

    private void Update()
    {
        if (skillActivationMap[Skill.Automation])
        {
            ExecuteAutomation();
        }

        if (skillActivationMap[Skill.Echo])
        {
            ExecuteEcho();
        }
    }

    public bool ExecuteSkill(Skill skill)
    {
        bool isExecuted = false;
        switch (skill)
        {
            case Skill.Automation:
                skillActivationMap[Skill.Automation] = true;
                isExecuted = true;
                break;
            case Skill.Echo:
                skillActivationMap[Skill.Echo] = true;
                isExecuted = true;
                break;
            case Skill.TimeMaster:
                isExecuted = ExecuteTimeMaster();
                break;
            default:
                Debug.Log("Skills Executor: Unknown skill name: " + skill);
                break;
        }
        
        return isExecuted;
    }

    private bool RemoveNormalBubbles(int count)
    {
        // find all current normal bubbles in the scene
        GameObject[] normals = GameObject.FindGameObjectsWithTag(Bubble.NormalBubble.ToString());
        if (normals == null || normals.Length == 0) return false;

        // remove up to 'count' of them this tick (random selection to spread out)
        int toRemove = Mathf.Min(count, normals.Length);
        for (int i = 0; i < toRemove; i++)
        {
            int idx = UnityEngine.Random.Range(0, normals.Length);
            GameObject chosen = normals[idx];

            // compact the array so we don't pick the same one twice
            normals[idx] = normals[normals.Length - 1];
            Array.Resize(ref normals, normals.Length - 1);

            if (chosen != null)
            {
                bubblesManager.RemoveBubble(chosen); // this also respawns based on your implementation
                ProgressBarFill.Instance.UpdateCurrentScore(bubblesManager.bubbleScoreMap[Bubble.NormalBubble]);
            }
        }
        
        return true;
    }

    // passive skills
    private bool ExecuteAutomation()
    {
        if (SkillsBinder.Instance.IsUnityNull()) return false;

        // accumulate time; only fire once per interval
        automationTimer += Time.deltaTime;
        if (automationTimer < automationInterval) return false;
        automationTimer -= automationInterval;

        int count = SkillsBinder.Instance.skillsStackMap[Skill.Automation];
        if (count <= 0) return false;

        var bm = bubblesManager;
        if (bm == null)
        {
            Debug.LogError("SkillsExecutor: BubblesManager script missing or wrong type.");
            return false;
        }

        return RemoveNormalBubbles(count);
    }

    // active skills
    private bool ExecuteEcho()
    {
        if (player.hasCollidedWithBubbles)
        {
            if (SkillsBinder.Instance.IsUnityNull()) return false;
            
            int count = 3 + SkillsBinder.Instance.skillsStackMap[Skill.Echo];
            RemoveNormalBubbles(count);
            skillActivationMap[Skill.Echo] = false;
        }
        
        return true;
    }

    private bool ExecuteTimeMaster()
    {
        if (Timer.Instance.IsUnityNull() || SkillsBinder.Instance.IsUnityNull()) return false;

        float amount = timeMasterDuration + SkillsBinder.Instance.skillsStackMap[Skill.TimeMaster];
        Timer.Instance.FreezeTimer(amount);
        return true;
    }
}
