using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FallbackSlotContainer : MonoBehaviour
{
    public abstract void UpdateAllSlots();
    public abstract Slot FindFreeSlotFor(DraggableModel model);
    public abstract IEnumerable<Slot> GetAllSlots();

    public bool HasSlot(Slot slot)
    {
        foreach(Slot nextSlot in GetAllSlots())
        {
            if (slot == nextSlot)
            {
                return true;
            }
        }
        return false;
    }
}
