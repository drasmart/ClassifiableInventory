using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FallbackSlotContainer))]
public class ForbiddenItems : MonoBehaviour
{
    public List<ItemType> blacklist;

    private FallbackSlotContainer slotsContainer;

    private void Start()
    {
        slotsContainer = GetComponent<FallbackSlotContainer>();
    }

    public void Validate(DropTransaction transaction)
    {
        if (!enabled || blacklist == null || !transaction.valid || transaction.dropSlot == null)
        {
            return;
        }
        var mainItemType = (transaction.draggableUI.draggableModel as Item)?.itemType;
        if (mainItemType != null)
        {
            if (blacklist.Contains(mainItemType) && slotsContainer.HasSlot(transaction.dropSlot))
            {
                transaction.Invalidate();
                return;
            }
        }
        var swapItemType = (transaction.dropSlot.draggableModel as Item)?.itemType;
        if (swapItemType != null && transaction.fallbackSlot != null)
        {
            if (blacklist.Contains(swapItemType) && slotsContainer.HasSlot(transaction.fallbackSlot))
            {
                transaction.Invalidate();
                return;
            }
        }
    }
}
