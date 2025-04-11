#nullable enable

public class DropTransaction
{
    public readonly DraggableUI DraggableUI;
    public readonly Slot? DropSlot;
    public readonly Slot? FallbackSlot;

    public bool Valid { get; private set; }

    public DropTransaction(DraggableUI draggableUI, Slot? dropSlot, Slot? fallbackSlot, bool valid)
    {
        DraggableUI = draggableUI;
        DropSlot = dropSlot;
        FallbackSlot = fallbackSlot;
        Valid = valid;
    }

    public void Invalidate() { Valid = false; }
}
