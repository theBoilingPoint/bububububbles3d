using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "ScriptableObjects/Skill")]
public class SkillScriptable : ScriptableObject
{
    public enum Type
    {
        Active,
        Passive
    }
    
    [Header("Gameplay")]
    public Type type;
    
    [Header("UI")]
    public Sprite image;
    public string skillName;
    public string activationDescription;
    public string stackingDescription;
}
