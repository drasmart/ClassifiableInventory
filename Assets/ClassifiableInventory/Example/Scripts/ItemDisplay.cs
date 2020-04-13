using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemDisplay : MonoBehaviour
{
    public Image icon;

    public void UpdateUI(DraggableModel draggableModel)
    {
        var item = draggableModel as Item;
        if (icon)
        {
            icon.sprite = item?.itemType?.sprite;
        }
    }
}
