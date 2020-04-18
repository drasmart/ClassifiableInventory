using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Inventory/Item Type")]
public class ItemType : ScriptableObject, SpritedAsset
{
    public ItemSlotTypeAsset[] slotTypes;
    public Sprite sprite;
    [Min(0)]
    public int durability;
    [Min(0)]
    public int stackSize;

    public Sprite previewSprite => sprite;
}
