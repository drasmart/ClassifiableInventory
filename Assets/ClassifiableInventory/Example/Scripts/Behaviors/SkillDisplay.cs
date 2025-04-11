using UnityEngine;
using UnityEngine.UI;

#nullable enable

public class SkillDisplay : MonoBehaviour
{
    public Image? iconImage;

    public void UpdateUI(IDraggableModel draggableModel, bool isShadow)
    {
        var item = draggableModel as Skill;
        if (iconImage)
        {
            iconImage.sprite = item?.skillType?.sprite;
        }
    }
}
