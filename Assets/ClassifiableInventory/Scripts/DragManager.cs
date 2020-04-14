using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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

    [Header("Prefabs")]
    public GameObject[] dragItemPrefabs;

    [Header("Debug")]
    public RectTransform lastEventImage;

    private Canvas canvas;

    private List<RaycastResult> raycastResults = new List<RaycastResult>();
    private Vector2 lastEventPosition;

    [Header("Drag Events")]
    public DragStartedEvent onDragStarted;
    public DragMovedEvent onDragMoved;
    public DragMovedEvent onDragDropped;
    public DragEndedEvent onDragEnded;
    public DragCancelledEvent onDragCancelled;

    [Header("Validation")]
    public DropTransactionValidationEvent validationEvent;

    [Header("Combination")]
    public DropTransactionValidationEvent combinationEvent;

    [System.Serializable] public class DragStartedEvent : UnityEvent<PointerEventData, DraggableUI> { }
    [System.Serializable] public class DropTransactionValidationEvent : UnityEvent<DropTransaction> { }
    [System.Serializable] public class DragMovedEvent : UnityEvent<PointerEventData, DropTransaction> { }
    [System.Serializable] public class DragEndedEvent : UnityEvent<PointerEventData, DraggableUI> { }
    [System.Serializable] public class DragCancelledEvent : UnityEvent<DraggableUI> { }

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
        }
        else
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
            onDragStarted?.Invoke(eventData, draggedItem);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Report("OnDrag", eventData);
        HandleDrag(eventData);
    }
    private void HandleDrag(PointerEventData eventData)
    {
        if (draggedTransform == null)
        {
            return;
        }
        draggedTransform.position = dragOffset + GetDragPoint(eventData);
        if (onDragMoved != null)
        {
            var dropTransaction = MakeDropTransaction(eventData);
            onDragMoved.Invoke(eventData, dropTransaction);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Report("OnEndDrag", eventData);
        if (!draggedTransform)
        {
            return;
        }
        var dropTransaction = MakeDropTransaction(eventData);
        if (dropTransaction.valid)
        {
            combinationEvent?.Invoke(dropTransaction);
            if (!dropTransaction.valid)
            {
                if (draggedItem?.slot != null)
                {
                    draggedItem.slot?.UpdateAllSlots();
                    if (draggedItem?.slot != null)
                    {
                        Attach(draggedItem, draggedItem.slot, false);
                    }
                }
                draggedItem = null;
                onDragCancelled?.Invoke(dropTransaction.draggableUI);
                return;
            }
        }
        if (dropTransaction.valid)
        {
            DoDrop(dropTransaction);
            draggedItem = null;
            onDragDropped?.Invoke(eventData, dropTransaction);
            return;
        }
        Attach(draggedItem, draggedItem.slot, false);
        draggedItem = null;
        onDragEnded?.Invoke(eventData, dropTransaction.draggableUI);

    }
    #endregion

    public void RefreshDrag()
    {
        if (draggedItem != null)
        {
            var fakeEvent = new PointerEventData(EventSystem.current);
            fakeEvent.position = lastEventPosition;
            HandleDrag(fakeEvent);
        }
    }

    private DropTransaction MakeDropTransaction(PointerEventData eventData)
    {
        var dropCandidateSlot = GetFirstHit<Slot>(eventData, dragOffset);
        if (dropCandidateSlot == null || draggedItem.draggableModel == null)
        {
            // no slot to drop, or no model to check acceptance
            return new DropTransaction(draggedItem, null, null, false);
        }
        if (dropCandidateSlot != null)
        {
            if (dropCandidateSlot == draggedItem.slot)
            {
                // drop on source
                return new DropTransaction(draggedItem, dropCandidateSlot, null, false);
            }
            if (!SlotAcceptsValue(dropCandidateSlot, draggedItem.draggableModel))
            {
                // slot does not accept dragged model
                return new DropTransaction(draggedItem, dropCandidateSlot, null, false);
            }
        }
        Slot dropFallbackSlot = null;
        var oldDraggable = dropCandidateSlot?.draggableUI;
        if (draggedItem.draggableModel == null || !SlotAcceptsValue(dropCandidateSlot, draggedItem.draggableModel))
        {
            // Can't accept draggable in this slot
            return new DropTransaction(draggedItem, dropCandidateSlot, null, false);
        }
        var oldSlot = draggedItem?.slot;
        bool isSwap = oldDraggable;
        if (isSwap)
        {
            var swapModel = oldDraggable.draggableModel;
            if (swapModel == null)
            {
                // candidate slot has draggable without model
                return new DropTransaction(draggedItem, dropCandidateSlot, null, false);
            }
            do
            {
                if (oldSlot != null && SlotAcceptsValue(oldSlot, swapModel))
                {
                    dropFallbackSlot = oldSlot;
                    break;
                }
                var primaryFallbackSlot = dropCandidateSlot.fallbackSlotContainer?.FindFreeSlotFor(swapModel);
                if (primaryFallbackSlot != null && SlotAcceptsValue(primaryFallbackSlot, swapModel))
                {
                    dropFallbackSlot = primaryFallbackSlot;
                    break;
                }
                var otherFallbackSlot = oldSlot?.fallbackSlotContainer?.FindFreeSlotFor(swapModel);
                if (otherFallbackSlot != null && SlotAcceptsValue(otherFallbackSlot, swapModel))
                {
                    dropFallbackSlot = otherFallbackSlot;
                    break;
                }
                // fallback slot not found
                return new DropTransaction(draggedItem, dropCandidateSlot, null, false);
            } while (false);
        }
        var result = new DropTransaction(draggedItem, dropCandidateSlot, dropFallbackSlot, true);
        validationEvent?.Invoke(result);
        return result;
    }
    private void DoDrop(DropTransaction transaction)
    {
        var oldSlot = transaction.draggableUI.slot;
        var oldDraggable = transaction.dropSlot.draggableUI;
        Attach(transaction.draggableUI, transaction.dropSlot, true);
        transaction.dropSlot.draggableModel = transaction.draggableUI.draggableModel;
        if (oldSlot)
        {
            oldSlot.draggableModel = null;
        }
        if (oldDraggable != null && transaction.fallbackSlot != null)
        {
            Attach(oldDraggable, transaction.fallbackSlot, true);
            transaction.fallbackSlot.draggableModel = oldDraggable.draggableModel;
        }
    }

    #region Game Object linking
    private void Attach(DraggableUI draggable, Slot slot, bool link)
    {
        var dragTransform = draggable.transform as RectTransform;
        var slotTransform = slot.draggableContainer ?? (slot.transform as RectTransform);
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
            if (draggedItem != null && slot.draggableUI == draggedItem)
            {
                var reportedItem = draggedItem;
                draggedItem = null;
                onDragCancelled?.Invoke(reportedItem);
            }
            return;
        }
        var draggableUI = slot.draggableUI;
        var model = slot.draggableModel;
        if (model == null || model.IsNull)
        {
            if (draggableUI != null)
            {
                if (draggedItem != null && draggableUI == draggedItem)
                {
                    var reportedItem = draggedItem;
                    draggedItem = null;
                    onDragCancelled?.Invoke(reportedItem);
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
            draggableUI.onModelUpdate?.Invoke(model);
        }
    }
    private DraggableUI SpawnDraggableUI(Slot slot)
    {
        var classes = slot.draggableModel.classes;
        foreach (var nextPrefab in dragItemPrefabs)
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
    public static bool SlotAcceptsValue(Slot slot, DraggableModel model)
    {
        if (slot == null || model == null)
        {
            return false;
        }
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


    private T GetFirstHit<T>(PointerEventData eventData, Vector3? localOffset = null) where T : MonoBehaviour
    {
        PointerEventData testEvent;
        if (localOffset == null)
        {
            testEvent = eventData;
        }
        else
        {
            testEvent = new PointerEventData(EventSystem.current);
            testEvent.position = eventData.position + GetEventPointDelta(localOffset.Value);
        }
        EventSystem.current.RaycastAll(testEvent, raycastResults);
        foreach (var nextResult in raycastResults)
        {
            var t = nextResult.gameObject.GetComponentInParent<T>();
            if (t != null)
            {
                return t;
            }
        }
        return null;
    }

    private void Report(string method, PointerEventData eventData)
    {
        lastEventPosition = eventData.position;
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
