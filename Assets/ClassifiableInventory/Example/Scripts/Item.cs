using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Classification;


[System.Serializable]
public class Item : DraggableModel
{
    public ItemType itemType;
    [Min(0)]
    public int durability;
    [Min(0)]
    public int count;

    public Classifiable.TypeAsset[] classes {
        get {
            var slots = itemType?.slotTypes;
            if (slots == null)
            {
                return new Classifiable.TypeAsset[0];
            }
            return Array.ConvertAll<ItemSlotTypeAsset, Classifiable.TypeAsset>(slots, (slotType) => slotType);
        }
    }

    public bool IsNull { get { return itemType == null || count == 0; } }

    public int? FreeCapacity {
        get {
            if (itemType == null || itemType.stackSize == 0)
            {
                return null;
            }
            return itemType.stackSize - count;
        }
    }

    public Item(ItemType itemType, int durability, int count)
    {
        this.itemType = itemType;
        this.durability = durability;
        this.count = count;
    }
    public Item(ItemType itemType) : this(itemType, itemType.durability, 1) { }
    public Item() : this(null, 0, 0) { }

    public int FreeSlotsFor(Item other)
    {
        var otherType = other?.itemType;
        if (itemType != null && itemType == otherType)
        {
            return FreeCapacity ?? other.count;
        }
        return 0;
    }
}
