using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AspectRatioFitter))]
public class DragManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static DragManager instance { get; private set; }

    public DraggableUI draggedItem;
    public RectTransform draggedTransform;
    public Vector3 dragOffset;

    public RectTransform lastEventImage;

    private Canvas canvas;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        if (instance == this)
        {
            return;
        }
        if (instance == null)
        {
            instance = this;
        } else
        {
            Debug.LogWarning("[" + transform.name + "] DragManager.instance already assigned to '" + instance.transform.name + "'");
            enabled = false;
        }
    }

    private List<RaycastResult> raycastResults = new List<RaycastResult>();

    public void OnBeginDrag(PointerEventData eventData)
    {
        Report("OnBeginDrag", eventData);
        draggedItem = GetFirstHit<DraggableUI>(eventData);
        if (draggedItem != null)
        {
            draggedTransform = draggedItem.transform as RectTransform;
            var dragPoint = GetDragPoint(eventData);
            dragOffset = draggedTransform.position - dragPoint;
            Detach(draggedItem, false);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Report("OnDrag", eventData);
        if (draggedTransform != null)
        {
            draggedTransform.position = dragOffset + GetDragPoint(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Report("OnEndDrag", eventData);
        if (!draggedTransform)
        {
            return;
        }
        var slot = GetFirstHit<Slot>(eventData, dragOffset);
        if (slot)
        {

            DoDrop(draggedItem, slot);
        }
        draggedItem = null;
        draggedTransform = null;
    }

    private void DoDrop(DraggableUI draggable, Slot slot)
    {
        var oldDraggable = slot.draggableUI;
        var oldSlot = draggable.slot;
        Attach(draggable, slot);
        if (oldDraggable && oldSlot)
        {
            Attach(oldDraggable, oldSlot);
        }
    }

    private void Attach(DraggableUI draggable, Slot slot)
    {
        var dragTransform = draggable.transform as RectTransform;
        var slotTransform = slot.transform as RectTransform;
        dragTransform.SetParent(slotTransform, true);
        dragTransform.localScale = Vector3.one;
        var s = slotTransform.rect.size;
        var a = slotTransform.pivot;
        var da = Vector2.one / 2 - a;
        var p = new Vector2(s.x * da.x, s.y * da.y);
        dragTransform.localPosition = p;
        if (draggable.slot)
        {
            draggable.slot.draggableUI = null;
        }
        draggable.slot = slot;
        if (slot.draggableUI)
        {
            Detach(slot.draggableUI, true);
        }
        slot.draggableUI = draggable;
    }

    private void Detach(DraggableUI draggable, bool unlink)
    {
        if (draggable == null)
        {
            return;
        }
        if (unlink && draggable.slot != null)
        {
            draggable.slot.draggableUI = null;
            draggable.slot = null;
        }
        draggable.transform.SetParent(transform, true);
        draggable.transform.SetAsLastSibling();
        lastEventImage?.SetAsLastSibling();
    }

    private T GetFirstHit<T>(PointerEventData eventData, Vector3? localOffset = null) where T: MonoBehaviour
    {
        PointerEventData testEvent;
        if (localOffset == null)
        {
            testEvent = eventData;
        } else
        {
            testEvent = new PointerEventData(EventSystem.current);
            testEvent.position = eventData.position + GetEventPointDelta(localOffset.Value);
        }
        EventSystem.current.RaycastAll(testEvent, raycastResults);
        foreach(var nextResult in raycastResults)
        {
            var t = nextResult.gameObject.GetComponent<T>();
            if (t != null)
            {
                return t;
            }
        }
        return null;
    }

    private void Report(string method, PointerEventData eventData)
    {
        //Debug.Log("[" + method + "]: " + eventData.ToString());

        var lastEventPoint = GetDragPoint(eventData);
        //Debug.Log(lastEventPoint);
        if (lastEventImage != null)
        {
            lastEventImage.position = lastEventPoint;
        }
    }

    private Vector3 GetDragPoint(PointerEventData eventData)
    {
        return transform.InverseTransformPoint(canvas.transform.TransformPoint(eventData.position));
    }
    private Vector2 GetEventPointDelta(Vector3 localOffset)
    {
        return canvas.transform.InverseTransformVector(transform.TransformVector(localOffset));
    }
}
