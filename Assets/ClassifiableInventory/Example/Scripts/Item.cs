using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Classification;


[System.Serializable]
public class Item : DraggableModel
{
    public ItemType itemType;
    public int durability;

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

    public bool IsNull { get { return itemType == null; } }
}
