using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SkillSlot : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private SkillScriptable skillScriptable;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private TextMeshProUGUI keyBindingText;
    [SerializeField] private Image refreshMask;
    
    [Header("Params")]
    [SerializeField] private float fillStep = 0.25f;   // how much to add each tick
    [SerializeField] private float stepInterval = 1f;  // seconds per tick
    
    private string skillName;
    private Image image;
    private int count = 0;
    private Key keyBinding = Key.None;
    
    private Coroutine fillRoutine;
    private bool isPressed = false;
    private bool isRefreshing = false;

    void Awake()
    {
        image = GetComponent<Image>();
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current != null && keyBinding != Key.None)
        {
            if (Keyboard.current[keyBinding].wasPressedThisFrame && !isRefreshing && !isPressed)
            {
                fillRoutine = StartCoroutine(FillMaskRoutine());
            }
        }
    }
    
    private IEnumerator FillMaskRoutine()
    {
        isRefreshing = true;
        isPressed = true;

        refreshMask.gameObject.SetActive(true);
        refreshMask.fillAmount = 1f; 

        while (refreshMask.fillAmount > 0f)
        {
            yield return new WaitForSeconds(stepInterval);
            refreshMask.fillAmount = Mathf.Max(0f, refreshMask.fillAmount - fillStep);
        }

        refreshMask.gameObject.SetActive(false);

        isPressed = false;
        isRefreshing = false;
        fillRoutine = null;
    }
    
    public void InitializeSkill(SkillScriptable newSkill, Key binding = Key.None)
    {
        skillName = newSkill.skillName;
        skillScriptable = newSkill;
        image.sprite = newSkill.image;
        refreshMask.gameObject.SetActive(false);
        count++;
        countText.gameObject.SetActive(false);
        if (binding != Key.None)
        {
            keyBinding = binding;
            keyBindingText.text = keyBinding.ToString();
        }
        else
        {
            keyBindingText.gameObject.SetActive(false);
        }
    }
    
    public void StackSkill(int skillCount)
    {
        count = skillCount;
        countText.text = count.ToString();
        countText.gameObject.SetActive(true);
    }

    public SkillScriptable GetSkillScriptable()
    {
        return skillScriptable;
    }

    public void SetSkillScriptable(SkillScriptable newSkill)
    {
        skillScriptable = newSkill;
    }
}
