using System;
using UnityEngine;
using drasmart.Classification;

#nullable enable

[Serializable]
public class Item : IDraggableModel
{
    public ItemType? itemType;
    [Min(0)]
    public int durability;
    [Min(0)]
    public int count;

    public Classifiable.TypeAsset[] Classes {
        get {
            var slots = itemType?.slotTypes;
            if (slots == null)
            {
                return Array.Empty<Classifiable.TypeAsset>();
            }
            return Array.ConvertAll<ItemSlotTypeAsset, Classifiable.TypeAsset>(slots, (slotType) => slotType);
        }
    }

    public bool IsNull => !itemType || count == 0;

    public int? FreeCapacity {
        get {
            if (!itemType || itemType.stackSize == 0)
            {
                return null;
            }
            return itemType.stackSize - count;
        }
    }

    public Item(ItemType? itemType, int durability, int count)
    {
        this.itemType = itemType;
        this.durability = durability;
        this.count = count;
    }
    public Item(ItemType itemType) : this(itemType, itemType.durability, 1) { }
    public Item() : this(null, 0, 0) { }

    public int FreeSlotsFor(Item other)
    {
        var otherType = other.itemType;
        if (itemType && itemType == otherType)
        {
            return FreeCapacity ?? other.count;
        }
        return 0;
    }
}
