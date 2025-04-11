using System;
using UnityEngine;

#nullable enable

[CreateAssetMenu(menuName = "Inventory/Skill Type")]
public class SkillType : ScriptableObject, ISpritedAsset
{
    public SkillTypeAsset[] slotTypes = Array.Empty<SkillTypeAsset>();
    public Sprite? sprite;
    [Min(0)]
    public float baseManaCost;

    public Sprite? PreviewSprite => sprite;
}
