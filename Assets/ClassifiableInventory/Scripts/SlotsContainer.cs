using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotsContainer : FallbackSlotContainer
{
    public List<FallbackSlotContainer> subSlots;

    public override Slot FindFreeSlotFor(DraggableModel model)
    {
        foreach(var nextSlot in subSlots)
        {
            var dst = nextSlot.FindFreeSlotFor(model);
            if (dst)
            {
                return dst;
            }
        }
        return null;
    }
}
