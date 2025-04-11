using System.Collections.Generic;

#nullable enable

public class SlotsContainer : FallbackSlotContainer
{
    public List<FallbackSlotContainer> subSlots = new();

    public override void UpdateAllSlots()
    {
        foreach(var nextSubSlot in subSlots)
        {
            nextSubSlot?.UpdateAllSlots();
        }
    }
    public override Slot? FindFreeSlotFor(IDraggableModel model)
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
    public override IEnumerable<Slot> GetAllSlots()
    {
        foreach(var nextSlot in subSlots)
        {
            foreach (var subSlot in nextSlot.GetAllSlots()) {
                yield return subSlot;
            }
        }
    }
}
