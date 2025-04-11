using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable

public abstract class FallbackSlotContainer : MonoBehaviour
{
    public abstract void UpdateAllSlots();
    public abstract Slot? FindFreeSlotFor(IDraggableModel model);
    public abstract IEnumerable<Slot> GetAllSlots();

    public bool HasSlot(Slot slot) => GetAllSlots().Contains(slot);
}
