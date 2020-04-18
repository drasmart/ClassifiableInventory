using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SlotsContainer))]
public class SlotsHighlighter : MonoBehaviour
{
    private SlotsContainer slotsContainer;

    private void Awake()
    {
        slotsContainer = GetComponent<SlotsContainer>();
    }

    public void OnDragStarted(PointerEventData eventData, DraggableUI draggableUI)
    {
        if (!(draggableUI.draggableModel is Item))
        {
            return;
        }
        ForEachSlot((slot, display) =>
        {
            if (slot == draggableUI.slot)
            {
                display.backgroundImage.color = display.backgroundColors.source;
            } else if (!DragManager.SlotAcceptsValue(slot, draggableUI.draggableModel))
            {
                display.nonAcceptingOverlay.SetActive(true);
            } else
            {
                display.backgroundImage.color = display.backgroundColors.normal;
            }
        });
    }
    public void OnDragMoved(PointerEventData eventData, DropTransaction transaction)
    {
        if (!(transaction.draggableUI.draggableModel is Item))
        {
            return;
        }
        bool dropSlotAccepts = DragManager.SlotAcceptsValue(transaction.dropSlot, transaction.draggableUI.draggableModel);
        ForEachSlot((slot, display) =>
        {
            var colors = display.backgroundColors;
            if (transaction.valid)
            {
                if (slot == transaction.draggableUI.slot)
                {
                    display.backgroundImage.color = (slot == transaction.fallbackSlot) ? colors.swapSource : colors.source;
                }
                else if (slot == transaction.dropSlot)
                {
                    display.backgroundImage.color = (slot.draggableModel == null || slot.draggableModel.IsNull) ? colors.dropDestination : colors.swapDestination;
                }
                else
                {
                    display.backgroundImage.color = (slot == transaction.fallbackSlot) ? colors.swapFallback : colors.normal;
                }
            } else
            {
                if (slot == transaction.draggableUI.slot)
                {
                    display.backgroundImage.color = (slot == transaction.dropSlot || transaction.dropSlot == null || !dropSlotAccepts) ? colors.source : colors.invalidSource;
                }
                else if (slot == transaction.dropSlot)
                {
                    display.backgroundImage.color = dropSlotAccepts ? colors.invalidDestination : colors.normal;
                }
                else
                {
                    display.backgroundImage.color = colors.normal;
                }
            }
        });
    }
    public void OnDragDropped(PointerEventData eventData, DropTransaction transaction)
    {
        OnDragCancelled(transaction.draggableUI);
    }
    public void OnDragEnded(PointerEventData eventData, DraggableUI draggableUI)
    {
        OnDragCancelled(draggableUI);
    }
    public void OnDragCancelled(DraggableUI draggableUI)
    {
        if (!(draggableUI.draggableModel is Item))
        {
            return;
        }
        ForEachSlot((slot, display) =>
        {
            display.backgroundImage.color = display.backgroundColors.passive;
            display.nonAcceptingOverlay.SetActive(false);
        });
    }

    private void ForEachSlot(UnityAction<Slot, SlotDisplay> action)
    {
        if (slotsContainer == null)
        {
            return;
        }
        foreach(var nextSlot in slotsContainer.GetAllSlots())
        {
            var nextDisplay = nextSlot.GetComponent<SlotDisplay>();
            if (nextDisplay)
            {
                action(nextSlot, nextDisplay);
            }
        }
    }
}
