using UnityEngine;

public class ItemStackCombiner : MonoBehaviour
{
    public void OnStackAttempt(DropTransaction transaction)
    {
        if (!transaction.Valid)
        {
            return;
        }
        var srcStack = transaction.DraggableUI.DraggableModel as Item;
        var dstStack = transaction.DropSlot.DraggableModel as Item;

        if (srcStack == null || dstStack == null || transaction.DraggableUI.slot == transaction.DropSlot)
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
                var slot = transaction.DraggableUI.slot;
                if (slot)
                {
                    slot.DraggableModel = null;
                    slot.UpdateAllSlots();
                }
            }
            transaction.DropSlot.UpdateAllSlots();
            transaction.Invalidate();
        }
    }
}
