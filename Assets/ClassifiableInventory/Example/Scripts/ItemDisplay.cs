using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(DraggableUI))]
public class ItemDisplay : MonoBehaviour
{
    public Image icon;

    private DraggableUI draggableUI;
    public Item item;

    private void Awake()
    {
        draggableUI = GetComponent<DraggableUI>();
    }

    public void UpdateUI()
    {
        item = draggableUI.draggableModel as Item;
        if (icon)
        {
            icon.sprite = item?.itemType?.sprite;
        }
    }
}
