using System;
using UnityEngine;

#nullable enable

[CreateAssetMenu(menuName = "Inventory/Item Type")]
public class ItemType : ScriptableObject, ISpritedAsset
{
    public ItemSlotTypeAsset[] slotTypes = Array.Empty<ItemSlotTypeAsset>();
    public Sprite? sprite;
    [Min(0)]
    public int durability;
    [Min(0)]
    public int stackSize;

    public Sprite? PreviewSprite => sprite;
}
