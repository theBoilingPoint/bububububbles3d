using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlot : MonoBehaviour
{
    [SerializeField] private SkillScriptable skillScriptable;
    private Image image;

    void Awake()
    {
        image = GetComponent<Image>();
    }
    
    public void InitializeSkill(SkillScriptable newSkill)
    {
        skillScriptable = newSkill;
        image.sprite = newSkill.image;
    }
}
