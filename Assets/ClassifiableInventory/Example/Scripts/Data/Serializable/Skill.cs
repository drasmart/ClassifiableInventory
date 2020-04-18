using Classification;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Skill : DraggableModel
{
    public SkillType skillType;
    [Min(0)]
    public int level = 0;

    public bool IsNull => skillType == null;

    public Classifiable.TypeAsset[] classes {
        get {
            var slots = skillType?.slotTypes;
            if (slots == null)
            {
                return new Classifiable.TypeAsset[0];
            }
            return System.Array.ConvertAll<SkillTypeAsset, Classifiable.TypeAsset>(slots, (slotType) => slotType);
        }
    }
}
