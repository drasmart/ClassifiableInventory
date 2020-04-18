using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemStackCombiner : MonoBehaviour
{
    public void OnStackAttempt(DropTransaction transaction)
    {
        if (!transaction.valid)
        {
            return;
        }
        var srcStack = transaction.draggableUI.draggableModel as Item;
        var dstStack = transaction.dropSlot.draggableModel as Item;

        if (srcStack == null || dstStack == null || transaction.draggableUI.slot == transaction.dropSlot)
        {
            return;
        }
        var deltaSize = Mathf.Clamp(dstStack.FreeSlotsFor(srcStack), 0, Mathf.Min(srcStack.itemType?.stackSize ?? 0, srcStack.count));
        if (deltaSize > 0)
        {
            dstStack.count += deltaSize;
            srcStack.count -= deltaSize;
            if (srcStack.count == 0)
            {
                var slot = transaction.draggableUI.slot;
                if (slot)
                {
                    slot.draggableModel = null;
                    slot.UpdateAllSlots();
                }
            }
            transaction.dropSlot.UpdateAllSlots();
            transaction.Invalidate();
        }
    }
}
