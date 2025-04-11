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
                return;
            }
            if (!DragManager.SlotAcceptsValue(slot, draggableUI.DraggableModel))
            {
                display.nonAcceptingOverlay.SetActive(true);
                return;
            }
            if (slot.isReadOnly && draggableUI.slot?.isReadOnly == true)
            {
                display.nonAcceptingOverlay.SetActive(true);
                return;
            }
            display.backgroundImage.color = display.backgroundColors.normal;
        });
    }
    public void OnDragMoved(PointerEventData eventData, DropTransaction transaction)
    {
        bool dropSlotAccepts = DragManager.SlotAcceptsValue(transaction.DropSlot, transaction.DraggableUI.DraggableModel);
        bool fromReadOnly = (transaction.DraggableUI.slot?.isReadOnly == true);
        ForEachSlot((slot, display) =>
        {
            var colors = display.backgroundColors;
            if (transaction.Valid)
            {
                if (slot == transaction.DraggableUI.slot)
                {
                    display.backgroundImage.color = (slot == transaction.FallbackSlot) ? colors.swapSource : colors.source;
                }
                else if (slot.isReadOnly && fromReadOnly)
                {
                    return;
                }
                else if (slot == transaction.DropSlot)
                {
                    display.backgroundImage.color = (slot.DraggableModel == null || slot.DraggableModel.IsNull) ? colors.dropDestination : colors.swapDestination;
                }
                else
                {
                    display.backgroundImage.color = (slot == transaction.FallbackSlot) ? colors.swapFallback : colors.normal;
                }
            } else
            {
                if (slot == transaction.DraggableUI.slot)
                {
                    display.backgroundImage.color = (fromReadOnly || slot == transaction.DropSlot || transaction.DropSlot == null || !dropSlotAccepts) ? colors.source : colors.invalidSource;
                } 
                else if (slot.isReadOnly && fromReadOnly)
                {
                    return;
                }
                else if (slot == transaction.DropSlot)
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
        OnDragCancelled(transaction.DraggableUI);
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
