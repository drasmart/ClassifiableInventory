using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Classification;

[RequireComponent(typeof(AspectRatioFitter))]
public class DragManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static DragManager Instance {
        get {
            if (instance == null)
            {
                instance = FindObjectOfType<DragManager>();
            }
            return instance;
        }
    }
    private static DragManager instance;

    private DraggableUI draggedItem;
    private RectTransform draggedTransform { get { return draggedItem?.transform as RectTransform; } }
    private Vector3 dragOffset;

    public GameObject[] dragItemPrefabs;

    public RectTransform lastEventImage;

    private Canvas canvas;

    private List<RaycastResult> raycastResults = new List<RaycastResult>();

    #region Unity Signals
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
    #endregion

    #region Drag Events
    public void OnBeginDrag(PointerEventData eventData)
    {
        Report("OnBeginDrag", eventData);
        draggedItem = GetFirstHit<DraggableUI>(eventData);
        if (draggedItem != null)
        {
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
        bool dropped = false;
        if (slot)
        {
            dropped = TryDrop(draggedItem, slot);
        }
        if (!dropped)
        {
            Attach(draggedItem, draggedItem.slot, false);
        }
        draggedItem = null;
    }
    #endregion

    private bool TryDrop(DraggableUI draggable, Slot slot)
    {
        var oldDraggable = slot.draggableUI;
        if (draggable.draggableModel == null || !SlotAcceptsValue(slot, draggable.draggableModel))
        {
            // Can't accept draggable in this slot
            return false;
        }
        var oldSlot = draggable?.slot;
        Slot swapSlot = null;
        bool isSwap = oldDraggable;
        if (isSwap)
        {
            var swapModel = oldDraggable.draggableModel;
            if (swapModel == null)
            {
                return false;
            }
            do
            {
                if (oldSlot != null && SlotAcceptsValue(oldSlot, swapModel))
                {
                    swapSlot = oldSlot;
                    break;
                }
                var primaryFallbackSlot = slot.fallbackSlotContainer?.FindFreeSlotFor(swapModel);
                if (primaryFallbackSlot != null && SlotAcceptsValue(primaryFallbackSlot, swapModel))
                {
                    swapSlot = primaryFallbackSlot;
                    break;
                }
                var otherFallbackSlot = oldSlot?.fallbackSlotContainer?.FindFreeSlotFor(swapModel);
                if (otherFallbackSlot != null && SlotAcceptsValue(otherFallbackSlot, swapModel))
                {
                    swapSlot = otherFallbackSlot;
                    break;
                }
                return false;
            } while (false);
        }
        Attach(draggable, slot, true);
        slot.draggableModel = draggable.draggableModel;
        if (oldSlot)
        {
            oldSlot.draggableModel = null;
        }
        if (oldDraggable != null && swapSlot != null)
        {
            Attach(oldDraggable, swapSlot, true);
            swapSlot.draggableModel = oldDraggable.draggableModel;
        }
        return true;
    }

    #region Game Object linking
    private void Attach(DraggableUI draggable, Slot slot, bool link)
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
        if (!link)
        {
            return;
        }
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
    #endregion

    public void UpdateSlot(Slot slot, bool? activationFront)
    {
        if (!enabled)
        {
            return;
        }
        if (activationFront == false)
        {
            if (slot.draggableUI == draggedItem)
            {
                draggedItem = null;
            }
            return;
        }
        var draggableUI = slot.draggableUI;
        var model = slot.draggableModel;
        if (model == null || model.IsNull)
        {
            if (draggableUI != null)
            {
                if (draggableUI == draggedItem)
                {
                    draggedItem = null;
                }
                Detach(draggableUI, true);
                Despawn(draggableUI);
            }
            return;
        }
        if (draggableUI == null)
        {
            if (model == null)
            {
                return;
            }
            draggableUI = SpawnDraggableUI(slot);
        }
        if (draggableUI)
        {
            draggableUI.draggableModel = model;
            draggableUI.onModelUpdate?.Invoke();
        }
    }
    private DraggableUI SpawnDraggableUI(Slot slot)
    {
        var classes = slot.draggableModel.classes;
        foreach(var nextPrefab in dragItemPrefabs)
        {
            var draggable = nextPrefab.GetComponent<DraggableUI>();
            if (draggable && PrefabAcceptsClasses(nextPrefab, classes))
            {
                var clone = SpawnPrefab(draggable);
                Attach(clone, slot, true);
                return clone;
            }
        }
        return null;
    }
    private static bool SlotAcceptsValue(Slot slot, DraggableModel model)
    {
        return PrefabAcceptsClasses(slot.gameObject, model.classes) || !slot.CanAcceptValue(model.GetType());
    }
    private static bool PrefabAcceptsClasses(GameObject prefab, Classifiable.TypeAsset[] classes)
    {
        var classifiable = prefab.GetComponent<Classifiable>();
        foreach (var nextFilter in classifiable.AllClasses)
        {
            bool rejected = true;
            foreach (var nextClass in classes)
            {
                if (nextFilter.Filter(nextClass))
                {
                    rejected = false;
                    break;
                }
            }
            if (rejected)
            {
                return false;
            }
        }
        return true;
    }

    private DraggableUI SpawnPrefab(DraggableUI prefab)
    {
        // TODO: Move to pool
        return Instantiate(prefab);
    }

    private void Despawn(DraggableUI draggableUI)
    {
        // TODO: Move to pool
        Destroy(draggableUI.gameObject);
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

    #region Coordinates Conversion
    private Vector3 GetDragPoint(PointerEventData eventData)
    {
        return transform.InverseTransformPoint(canvas.transform.TransformPoint(eventData.position));
    }
    private Vector2 GetEventPointDelta(Vector3 localOffset)
    {
        return canvas.transform.InverseTransformVector(transform.TransformVector(localOffset));
    }
    #endregion
}
