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
    public void OnDragMoved(PointerEventData eventData, DraggableUI draggableUI, Slot dropSlot, Slot fallbackSlot)
    {
        ForEachSlot((slot, display) =>
        {
            var colors = display.backgroundColors;
            if (slot == draggableUI.slot)
            {
                display.backgroundImage.color = (slot == fallbackSlot) ? colors.swapSource : colors.source;
            } else if (slot == dropSlot)
            {
                display.backgroundImage.color = (slot.draggableModel == null || slot.draggableModel.IsNull) ? colors.dropDestination : colors.swapDestination;
            } else
            {
                display.backgroundImage.color = (slot == fallbackSlot) ? colors.swapFallback : colors.normal;
            }
        });
    }
    public void OnDragDropped(PointerEventData eventData, DraggableUI draggableUI, Slot dropSlot, Slot fallbackSlot)
    {
        OnDragCancelled(draggableUI);
    }
    public void OnDragEnded(PointerEventData eventData, DraggableUI draggableUI)
    {
        OnDragCancelled(draggableUI);
    }
    public void OnDragCancelled(DraggableUI draggableUI)
    {
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
