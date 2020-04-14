using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "NewItemType", menuName = "Inventory/Item Type")]
public class ItemType : ScriptableObject
{
    public ItemSlotTypeAsset[] slotTypes;
    public Sprite sprite;
    [Min(0)]
    public int durability;
    [Min(0)]
    public int stackSize;
}
