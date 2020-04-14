using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropTransaction
{
    public readonly DraggableUI draggableUI;
    public readonly Slot dropSlot;
    public readonly Slot fallbackSlot;

    public bool valid { get; private set; }

    public DropTransaction(DraggableUI draggableUI, Slot dropSlot, Slot fallbackSlot, bool valid)
    {
        this.draggableUI = draggableUI;
        this.dropSlot = dropSlot;
        this.fallbackSlot = fallbackSlot;
        this.valid = valid;
    }

    public void Invalidate() { valid = false; }
}
