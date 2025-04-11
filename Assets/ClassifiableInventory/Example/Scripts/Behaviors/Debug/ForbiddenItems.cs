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
        if (!enabled || blacklist == null || !transaction.Valid || transaction.DropSlot == null)
        {
            return;
        }
        var mainItemType = (transaction.DraggableUI.DraggableModel as Item)?.itemType;
        if (mainItemType != null)
        {
            if (blacklist.Contains(mainItemType) && slotsContainer.HasSlot(transaction.DropSlot))
            {
                transaction.Invalidate();
                return;
            }
        }
        var swapItemType = (transaction.DropSlot.DraggableModel as Item)?.itemType;
        if (swapItemType != null && transaction.FallbackSlot != null)
        {
            if (blacklist.Contains(swapItemType) && slotsContainer.HasSlot(transaction.FallbackSlot))
            {
                transaction.Invalidate();
                return;
            }
        }
    }
}
