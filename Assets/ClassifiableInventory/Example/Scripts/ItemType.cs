using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "NewItemType", menuName = "Inventory/Item Type")]
public class ItemType : ScriptableObject
{
    public ItemSlotTypeAsset slotType;
    public Sprite sprite;
    public int durability;
}
