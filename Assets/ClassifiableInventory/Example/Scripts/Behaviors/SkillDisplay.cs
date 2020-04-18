using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillDisplay : MonoBehaviour
{
    public Image iconImage;

    public void UpdateUI(DraggableModel draggableModel, bool isShadow)
    {
        var item = draggableModel as Skill;
        if (iconImage)
        {
            iconImage.sprite = item?.skillType?.sprite;
        }
    }
}
