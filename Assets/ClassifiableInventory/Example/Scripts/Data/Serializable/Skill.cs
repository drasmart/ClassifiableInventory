using System;
using Classification;
using UnityEngine;

#nullable enable

[System.Serializable]
public class Skill : IDraggableModel
{
    public SkillType? skillType;
    [Min(0)]
    public int level = 0;

    public bool IsNull => !skillType;

    public Classifiable.TypeAsset[] Classes {
        get {
            var slots = skillType?.slotTypes;
            if (slots == null)
            {
                return Array.Empty<Classifiable.TypeAsset>();
            }
            return Array.ConvertAll<SkillTypeAsset, Classifiable.TypeAsset>(slots, (slotType) => slotType);
        }
    }
}
