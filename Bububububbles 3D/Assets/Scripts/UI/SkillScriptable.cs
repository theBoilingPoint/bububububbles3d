using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum SkillType
{
    Active,
    Passive
}

[CreateAssetMenu(menuName = "ScriptableObjects/Skill")]
public class SkillScriptable : ScriptableObject
{
    [Header("Gameplay")]
    public TileBase tile;
    public SkillType skillType;
    
    [Header("UI")]
    public Sprite image;
}
