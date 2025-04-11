using System;
using UnityEngine;

#nullable enable

[CreateAssetMenu(menuName = "Inventory/Skill Types Collection")]
public class SkillTypesCollection : ScriptableObject
{
    public SkillType[] skillTypes = Array.Empty<SkillType>();
}
