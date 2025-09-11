using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillsSelectionMenu : MonoBehaviour
{
    public static SkillsSelectionMenu Instance { get; private set; }
    
    [SerializeField] private SkillScriptable[] skillsScriptables;
    [SerializeField] private Button[] skillButtons;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (skillsScriptables.Length < skillButtons.Length)
        {
            Debug.LogError("SkillsSelectionMenu: skillsScriptables.Length > skillsSlots.Length");
        }
    }

    // TODO: In the future, we might randomise skills selection
    public void InitializeSkillsSelectionMenu()
    {
        for (int i = 0; i < skillButtons.Length; i++)
        {
            Button button = skillButtons[i];
            SkillScriptable skill = skillsScriptables[i];
            
            Transform title = button.transform.Find("Title");
            if (title != null) title.GetComponent<TextMeshProUGUI>().text = skill.skillName;
            Transform type = button.transform.Find("Type");
            if (type != null) type.GetComponent<TextMeshProUGUI>().text = skill.type.ToString();
            Transform icon = button.transform.Find("Icon");
            if (icon != null) icon.GetComponent<Image>().sprite = skill.image;
            Transform activationDescription = button.transform.Find("ActivationDescription");
            if (activationDescription != null) activationDescription.GetComponent<TextMeshProUGUI>().text = skill.activationDescription;
            Transform stackingDescription = button.transform.Find("StackingDescription");
            if (stackingDescription != null) stackingDescription.GetComponent<TextMeshProUGUI>().text = skill.stackingDescription;

            switch (skill.skillName)
            {
                case "Automation":
                    button.onClick.AddListener(SkillsManager.Instance.BindAutomation);
                    break;
                case "Echo":
                    button.onClick.AddListener(SkillsManager.Instance.BindEcho);
                    break;
                case "Time Master":
                    button.onClick.AddListener(SkillsManager.Instance.BindTimeMaster);
                    break;
                default:
                    Debug.LogError("You haven't implemented the function for skill: " + skill.skillName);
                    break;
            }
        }
    }

    public void ResetSkillsSelectionMenu()
    {
        for (int i = 0; i < skillButtons.Length; i++)
        {
            Button button = skillButtons[i];
            button.onClick.RemoveAllListeners();
            
            Transform title = button.transform.Find("Title");
            if (title != null) title.GetComponent<TextMeshProUGUI>().text = "";
            Transform type = button.transform.Find("Type");
            if (type != null) type.GetComponent<TextMeshProUGUI>().text = "";
            Transform icon = button.transform.Find("Icon");
            if (icon != null) icon.GetComponent<Image>().sprite = null;
            Transform activationDescription = button.transform.Find("ActivationDescription");
            if (activationDescription != null) activationDescription.GetComponent<TextMeshProUGUI>().text = "";
            Transform stackingDescription = button.transform.Find("StackingDescription");
            if (stackingDescription != null) stackingDescription.GetComponent<TextMeshProUGUI>().text = "";
        }
    }
}
