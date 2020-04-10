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
            var slotType = itemType?.slotType;
            if (slotType != null)
            {
                return new Classifiable.TypeAsset[1] { slotType };
            }
            return new Classifiable.TypeAsset[0];
        }
    }
}
