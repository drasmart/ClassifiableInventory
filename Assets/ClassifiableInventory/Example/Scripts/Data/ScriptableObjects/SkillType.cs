using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Inventory/Skill Type")]
public class SkillType : ScriptableObject, SpritedAsset
{
    public SkillTypeAsset[] slotTypes;
    public Sprite sprite;
    [Min(0)]
    public float baseManaCost;

    public Sprite previewSprite => sprite;
}
