using System.Collections;
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

    private Image image;
    private int count = 0;
    private Key keyBinding = Key.None;
    
    private Coroutine executionRoutine;
    private bool isPressed = false;
    private bool isRefreshing = false;
    private bool isExecuted = false;

    void Awake()
    {
        image = GetComponent<Image>();
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (DynamicMenu.IsAnyMenuOpen)
            return;
        
        if (Keyboard.current != null && keyBinding != Key.None)
        {
            if (Keyboard.current[keyBinding].wasPressedThisFrame && !isRefreshing && !isPressed && !isExecuted)
            {
                executionRoutine = StartCoroutine(SkillRoutine());
            }
        }
    }
    
    private IEnumerator SkillRoutine()
    {
        isRefreshing = true;
        isPressed = true;
        isExecuted = SkillsExecutor.Instance.ExecuteSkill(skillScriptable.skill);

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
        isExecuted = false;
        executionRoutine = null;
    }
    
    public void InitializeSkill(SkillScriptable newSkill, Key binding = Key.None)
    {
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
            // If there's no key binding, the current skill is passive
            SkillsExecutor.Instance.ExecuteSkill(skillScriptable.skill);
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

    public void ResetSlot()
    {
        if (executionRoutine != null)
        {
            StopCoroutine(executionRoutine);
            executionRoutine = null;
        }

        isRefreshing = false;
        isPressed = false;
        isExecuted = false;

        if (refreshMask != null)
        {
            refreshMask.fillAmount = 0f;   
            refreshMask.gameObject.SetActive(false);
        }
    }
}
