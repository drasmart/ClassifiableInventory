using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FallbackSlotContainer : MonoBehaviour
{
    public abstract void UpdateAllSlots();
    public abstract Slot FindFreeSlotFor(DraggableModel model);
}
