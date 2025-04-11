using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

#nullable enable

[RequireComponent(typeof(FallbackSlotContainer))]
public class ForbiddenItems : MonoBehaviour
{
    public List<ItemType> blacklist = new();

    private FallbackSlotContainer? slotsContainer;

    private void Start()
    {
        slotsContainer = GetComponent<FallbackSlotContainer>();
        Assert.IsNotNull(slotsContainer);
    }

    public void Validate(DropTransaction transaction)
    {
        Assert.IsNotNull(slotsContainer);
        if (!enabled || !transaction.Valid || !transaction.DropSlot)
        {
            return;
        }
        var mainItemType = (transaction.DraggableUI.DraggableModel as Item)?.itemType;
        if (mainItemType != null)
        {
            if (blacklist.Contains(mainItemType) && slotsContainer!.HasSlot(transaction.DropSlot))
            {
                transaction.Invalidate();
                return;
            }
        }
        var swapItemType = (transaction.DropSlot.DraggableModel as Item)?.itemType;
        if (swapItemType != null && transaction.FallbackSlot != null)
        {
            if (blacklist.Contains(swapItemType) && slotsContainer!.HasSlot(transaction.FallbackSlot))
            {
                transaction.Invalidate();
            }
        }
    }
}
