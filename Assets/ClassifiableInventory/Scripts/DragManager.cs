using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using drasmart.Classification;
using UnityEngine.Assertions;

#nullable enable

[RequireComponent(typeof(AspectRatioFitter))]
public class DragManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static DragManager Instance {
        get {
            if (!_instance)
            {
                _instance = FindAnyObjectByType<DragManager>();
            }
            return _instance;
        }
    }
    private static DragManager? _instance;

    private DraggableUI? draggedItem;
    private DraggableUI? shadowClone;
    private Vector3 dragOffset;

    [Header("Prefabs")]
    [SerializeField] private GameObject[] dragItemPrefabs = Array.Empty<GameObject>();

    [Header("Debug")]
    [SerializeField] private RectTransform? lastEventImage;

    private Canvas? canvas;

    private readonly List<RaycastResult> raycastResults = new();
    private Vector2 lastEventPosition;

    [Header("Drag Events")]
    public DragStartedEvent? onDragStarted;
    public DragMovedEvent? onDragMoved;
    public DragMovedEvent? onDragDropped;
    public DragEndedEvent? onDragEnded;
    public DragCancelledEvent? onDragCancelled;

    [Header("Validation")]
    public DropTransactionValidationEvent? validationEvent;

    [Header("Combination")]
    public DropTransactionValidationEvent? combinationEvent;

    [Header("Debug")]
    [SerializeField] private bool logReportCalls;
    [SerializeField] private bool logReportPoints;

    [Serializable] public class DragStartedEvent : UnityEvent<PointerEventData, DraggableUI> { }
    [Serializable] public class DropTransactionValidationEvent : UnityEvent<DropTransaction> { }
    [Serializable] public class DragMovedEvent : UnityEvent<PointerEventData, DropTransaction> { }
    [Serializable] public class DragEndedEvent : UnityEvent<PointerEventData, DraggableUI> { }
    [Serializable] public class DragCancelledEvent : UnityEvent<DraggableUI> { }

    #region Unity Signals
    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        Assert.IsNotNull(canvas);
        if (_instance == this)
        {
            return;
        }
        if (!_instance)
        {
            _instance = this;
        }
        else
        {
            Debug.LogWarning("[" + transform.name + "] DragManager.instance already assigned to '" + _instance.transform.name + "'");
            enabled = false;
        }
    }
    #endregion

    #region Drag Events
    public void OnBeginDrag(PointerEventData eventData)
    {
        Report("OnBeginDrag", eventData);
        draggedItem = GetFirstHit<DraggableUI>(eventData);
        if (!draggedItem)
        {
            return;
        }
        if (draggedItem.slot?.keepShadowWhileDragging == true)
        {
            Assert.IsNotNull(draggedItem.slot.DraggableModel);
            shadowClone = SpawnDraggableUI(draggedItem.slot, false);
            if (shadowClone)
            {
                shadowClone.DraggableModel = draggedItem.slot.DraggableModel;
                shadowClone.onModelUpdate?.Invoke(draggedItem.slot.DraggableModel!, true);
            }
        }
        var dragPoint = GetDragPoint(eventData);
        dragOffset = draggedItem.transform.position - dragPoint;
        Detach(draggedItem, false);
        onDragStarted?.Invoke(eventData, draggedItem);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Report("OnDrag", eventData);
        HandleDrag(eventData);
    }
    private void HandleDrag(PointerEventData eventData)
    {
        if (!draggedItem)
        {
            return;
        }
        draggedItem.transform.position = dragOffset + GetDragPoint(eventData);
        if (onDragMoved != null)
        {
            var dropTransaction = MakeDropTransaction(eventData);
            onDragMoved.Invoke(eventData, dropTransaction);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Report("OnEndDrag", eventData);
        if (!draggedItem)
        {
            return;
        }
        var dropTransaction = MakeDropTransaction(eventData);
        if (dropTransaction.Valid)
        {
            combinationEvent?.Invoke(dropTransaction);
            if (!dropTransaction.Valid)
            {
                if (draggedItem?.slot != null)
                {
                    draggedItem.slot?.UpdateAllSlots();
                    if (draggedItem?.slot != null)
                    {
                        Attach(draggedItem, draggedItem.slot, false);
                    }
                }
                ClearDragData();
                onDragCancelled?.Invoke(dropTransaction.DraggableUI);
                return;
            }
        }
        if (dropTransaction.Valid)
        {
            var slotsToUpdate = DoDrop(dropTransaction);
            ClearDragData();
            onDragDropped?.Invoke(eventData, dropTransaction);
            foreach(var nextSlot in slotsToUpdate)
            {
                UpdateSlot(nextSlot, nextSlot.enabled && nextSlot.gameObject.activeInHierarchy);
            }
            return;
        }
        Assert.IsNotNull(draggedItem.slot);
        Attach(draggedItem, draggedItem.slot!, false);
        ClearDragData();
        onDragEnded?.Invoke(eventData, dropTransaction.DraggableUI);

    }
    #endregion

    public void RefreshDrag()
    {
        if (!draggedItem)
        {
            var fakeEvent = new PointerEventData(EventSystem.current)
            {
                position = lastEventPosition
            };
            HandleDrag(fakeEvent);
        }
    }

    private DropTransaction MakeDropTransaction(PointerEventData eventData)
    {
        Assert.IsNotNull(draggedItem);
        var dropCandidateSlot = GetFirstHit<Slot>(eventData, dragOffset);
        if (!dropCandidateSlot || draggedItem?.DraggableModel is null)
        {
            // no slot to drop, or no model to check acceptance
            return new DropTransaction(draggedItem!, null, null, false);
        }
        Assert.IsNotNull(dropCandidateSlot);
        if (dropCandidateSlot == draggedItem.slot)
        {
            // drop on source
            return new DropTransaction(draggedItem, dropCandidateSlot, null, false);
        }
        if (!SlotAcceptsValue(dropCandidateSlot, draggedItem.DraggableModel))
        {
            // slot does not accept dragged model
            return new DropTransaction(draggedItem, dropCandidateSlot, null, false);
        }
        Slot? dropFallbackSlot = null;
        var oldDraggable = dropCandidateSlot.draggableUI;
        if (draggedItem.DraggableModel == null || !SlotAcceptsValue(dropCandidateSlot, draggedItem.DraggableModel))
        {
            // Can't accept draggable in this slot
            return new DropTransaction(draggedItem, dropCandidateSlot, null, false);
        }
        var oldSlot = draggedItem?.slot;
        if (oldDraggable)
        {
            var swapModel = oldDraggable.DraggableModel;
            if (swapModel == null)
            {
                // candidate slot has draggable without model
                return new DropTransaction(draggedItem!, dropCandidateSlot, null, false);
            }

            dropFallbackSlot = ((Func<Slot, Slot?>)((dropCandidateSlot2) =>
            {
                if (oldSlot && SlotAcceptsValue(oldSlot, swapModel))
                {
                    return oldSlot;
                }
                var primaryFallbackSlot = dropCandidateSlot2.fallbackSlotContainer?.FindFreeSlotFor(swapModel);
                if (primaryFallbackSlot && SlotAcceptsValue(primaryFallbackSlot, swapModel))
                {
                    return primaryFallbackSlot;
                }
                var otherFallbackSlot = oldSlot?.fallbackSlotContainer?.FindFreeSlotFor(swapModel);
                if (otherFallbackSlot && SlotAcceptsValue(otherFallbackSlot, swapModel))
                {
                    return otherFallbackSlot;
                }
                return null;
            }))(dropCandidateSlot);

            if (!dropFallbackSlot)
            {
                // fallback slot not found
                return new DropTransaction(draggedItem!, dropCandidateSlot, null, false);
            }
        }
        if (draggedItem?.slot?.isReadOnly == true && dropCandidateSlot.isReadOnly)
        {
            return new DropTransaction(draggedItem, dropCandidateSlot, dropFallbackSlot, false);
        }
        var result = new DropTransaction(draggedItem!, dropCandidateSlot, dropFallbackSlot, true);
        validationEvent?.Invoke(result);
        return result;
    }
    private IEnumerable<Slot> DoDrop(DropTransaction transaction)
    {
        Assert.IsNotNull(transaction.DropSlot);
        var oldSlot = transaction.DraggableUI.slot;
        var oldDraggable = transaction.DropSlot!.draggableUI;
        Attach(transaction.DraggableUI, transaction.DropSlot, true);
        var slotsToUpdate = new List<Slot>();
        TryUpdateModel(transaction.DropSlot, transaction.DraggableUI.DraggableModel, slotsToUpdate);
        TryUpdateModel(oldSlot, null, slotsToUpdate);
        if (oldDraggable&& transaction.FallbackSlot)
        {
            Attach(oldDraggable, transaction.FallbackSlot, true);
            TryUpdateModel(transaction.FallbackSlot, oldDraggable.DraggableModel, slotsToUpdate);
        }
        return slotsToUpdate;
    }

    private void TryUpdateModel(Slot? slot, IDraggableModel? model, IList<Slot> failList)
    {
        if (!slot)
        {
            return;
        }
        if (!slot.isReadOnly)
        {
            slot.DraggableModel = model;
        }
        else
        {
            failList.Add(slot);
        }
    }

    #region Game Object linking
    private void Attach(DraggableUI draggable, Slot slot, bool link)
    {
        var dragTransform = draggable.transform as RectTransform;
        if (!dragTransform)
        {
            return;
        }
        var slotTransform = slot.draggableContainer;
        if (!slotTransform)
        {
            slotTransform = slot.transform as RectTransform;
            if (!slotTransform)
            {
                return;
            }
        }
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
        if (!draggable)
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
                ClearDragData();
                onDragCancelled?.Invoke(reportedItem);
            }
            return;
        }
        var draggableUI = slot.draggableUI;
        var model = slot.DraggableModel;
        if (model is null || model.IsNull)
        {
            if (draggableUI)
            {
                if (draggedItem && draggableUI == draggedItem)
                {
                    var reportedItem = draggedItem;
                    ClearDragData();
                    onDragCancelled?.Invoke(reportedItem);
                }
                Detach(draggableUI, true);
                Despawn(draggableUI);
            }
            return;
        }
        if (!draggableUI)
        {
            draggableUI = SpawnDraggableUI(slot, true);
        }
        if (draggableUI)
        {
            draggableUI.DraggableModel = model;
            draggableUI.onModelUpdate?.Invoke(model, false);
        }
        if (draggableUI == draggedItem && shadowClone != null)
        {
            shadowClone.DraggableModel = model;
            shadowClone.onModelUpdate?.Invoke(model, true);
        }
    }
    private DraggableUI? SpawnDraggableUI(Slot slot, bool link)
    {
        Assert.IsNotNull(slot.DraggableModel);
        var classes = slot.DraggableModel!.Classes;
        foreach (var nextPrefab in dragItemPrefabs)
        {
            var draggable = nextPrefab.GetComponent<DraggableUI>();
            if (draggable && PrefabAcceptsClasses(nextPrefab, classes))
            {
                var clone = SpawnPrefab(draggable);
                Attach(clone, slot, link);
                return clone;
            }
        }
        return null;
    }
    private void ClearDragData()
    {
        draggedItem = null;
        if (shadowClone != null)
        {
            Despawn(shadowClone);
            shadowClone = null;
        }
    }
    public static bool SlotAcceptsValue(Slot slot, IDraggableModel model)
    {
        if (!slot)
        {
            return false;
        }
        return PrefabAcceptsClasses(slot.gameObject, model.Classes) && slot.CanAcceptValue(model.GetType());
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

    private static DraggableUI SpawnPrefab(DraggableUI prefab)
    {
        // TODO: Move to pool
        return Instantiate(prefab);
    }

    private static void Despawn(DraggableUI draggableUI)
    {
        // TODO: Move to pool
        Destroy(draggableUI.gameObject);
    }


    private T? GetFirstHit<T>(PointerEventData eventData, Vector3? localOffset = null) where T : MonoBehaviour
    {
        PointerEventData testEvent;
        if (localOffset == null)
        {
            testEvent = eventData;
        }
        else
        {
            testEvent = new PointerEventData(EventSystem.current)
            {
                position = eventData.position + GetEventPointDelta(localOffset.Value)
            };
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
        if (logReportCalls)
        {
            Debug.Log($"[{method}]: {eventData}");
        }

        var lastEventPoint = GetDragPoint(eventData);
        if (logReportPoints)
        {
            Debug.Log($"lastEventPoint");
        }

        if (lastEventImage)
        {
            lastEventImage.position = lastEventPoint;
        }
    }

    #region Coordinates Conversion
    private Vector3 GetDragPoint(PointerEventData eventData)
    {
        Assert.IsNotNull(canvas);
        return transform.InverseTransformPoint(canvas!.transform.TransformPoint(eventData.position));
    }
    private Vector2 GetEventPointDelta(Vector3 localOffset)
    {
        Assert.IsNotNull(canvas);
        return canvas!.transform.InverseTransformVector(transform.TransformVector(localOffset));
    }
    #endregion
}
