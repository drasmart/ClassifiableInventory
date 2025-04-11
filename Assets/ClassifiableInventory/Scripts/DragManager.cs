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
            if (!_instance)
            {
                _instance = FindAnyObjectByType<DragManager>();
            }
            return _instance;
        }
    }
    private static DragManager _instance;

    private DraggableUI draggedItem;
    private DraggableUI shadowClone;
    private RectTransform DraggedTransform => draggedItem?.transform as RectTransform;
    private Vector3 dragOffset;

    [Header("Prefabs")]
    public GameObject[] dragItemPrefabs;

    [Header("Debug")]
    public RectTransform lastEventImage;

    private Canvas canvas;

    private readonly List<RaycastResult> raycastResults = new();
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

    [Header("Debug")]
    [SerializeField] private bool logReportCalls;
    [SerializeField] private bool logReportPoints;

    [System.Serializable] public class DragStartedEvent : UnityEvent<PointerEventData, DraggableUI> { }
    [System.Serializable] public class DropTransactionValidationEvent : UnityEvent<DropTransaction> { }
    [System.Serializable] public class DragMovedEvent : UnityEvent<PointerEventData, DropTransaction> { }
    [System.Serializable] public class DragEndedEvent : UnityEvent<PointerEventData, DraggableUI> { }
    [System.Serializable] public class DragCancelledEvent : UnityEvent<DraggableUI> { }

    #region Unity Signals
    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
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
        if (draggedItem == null)
        {
            return;
        }
        if (draggedItem.slot?.keepShadowWhileDragging == true)
        {
            shadowClone = SpawnDraggableUI(draggedItem.slot, false);
            if (shadowClone)
            {
                shadowClone.draggableModel = draggedItem.slot.draggableModel;
                shadowClone.onModelUpdate?.Invoke(shadowClone.draggableModel, true);
            }
        }
        var dragPoint = GetDragPoint(eventData);
        dragOffset = DraggedTransform.position - dragPoint;
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
        if (DraggedTransform == null)
        {
            return;
        }
        DraggedTransform.position = dragOffset + GetDragPoint(eventData);
        if (onDragMoved != null)
        {
            var dropTransaction = MakeDropTransaction(eventData);
            onDragMoved.Invoke(eventData, dropTransaction);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Report("OnEndDrag", eventData);
        if (!DraggedTransform)
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
                ClearDragData();
                onDragCancelled?.Invoke(dropTransaction.draggableUI);
                return;
            }
        }
        if (dropTransaction.valid)
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
        Attach(draggedItem, draggedItem.slot, false);
        ClearDragData();
        onDragEnded?.Invoke(eventData, dropTransaction.draggableUI);

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
        var dropCandidateSlot = GetFirstHit<Slot>(eventData, dragOffset);
        if (!dropCandidateSlot || draggedItem.draggableModel is null)
        {
            // no slot to drop, or no model to check acceptance
            return new DropTransaction(draggedItem, null, null, false);
        }
        if (dropCandidateSlot)
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
        var oldDraggable = dropCandidateSlot.draggableUI;
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

            dropFallbackSlot = ((System.Func<Slot, Slot>)((dropCandidateSlot2) =>
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
                return new DropTransaction(draggedItem, dropCandidateSlot, null, false);
            }
        }
        if (draggedItem?.slot?.isReadOnly == true && dropCandidateSlot.isReadOnly)
        {
            return new DropTransaction(draggedItem, dropCandidateSlot, dropFallbackSlot, false);
        }
        var result = new DropTransaction(draggedItem, dropCandidateSlot, dropFallbackSlot, true);
        validationEvent?.Invoke(result);
        return result;
    }
    private IEnumerable<Slot> DoDrop(DropTransaction transaction)
    {
        var oldSlot = transaction.draggableUI.slot;
        var oldDraggable = transaction.dropSlot.draggableUI;
        Attach(transaction.draggableUI, transaction.dropSlot, true);
        var slotsToUpdate = new List<Slot>();
        TryUpdateModel(transaction.dropSlot, transaction.draggableUI.draggableModel, slotsToUpdate);
        TryUpdateModel(oldSlot, null, slotsToUpdate);
        if (oldDraggable != null && transaction.fallbackSlot != null)
        {
            Attach(oldDraggable, transaction.fallbackSlot, true);
            TryUpdateModel(transaction.fallbackSlot, oldDraggable.draggableModel, slotsToUpdate);
        }
        return slotsToUpdate;
    }

    private void TryUpdateModel(Slot slot, DraggableModel model, IList<Slot> failList)
    {
        if (!slot)
        {
            return;
        }
        if (!slot.isReadOnly)
        {
            slot.draggableModel = model;
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
                ClearDragData();
                onDragCancelled?.Invoke(reportedItem);
            }
            return;
        }
        var draggableUI = slot.draggableUI;
        var model = slot.draggableModel;
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
            draggableUI.draggableModel = model;
            draggableUI.onModelUpdate?.Invoke(model, false);
        }
        if (draggableUI == draggedItem && shadowClone != null)
        {
            shadowClone.draggableModel = model;
            shadowClone.onModelUpdate?.Invoke(model, true);
        }
    }
    private DraggableUI SpawnDraggableUI(Slot slot, bool link)
    {
        var classes = slot.draggableModel.classes;
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
    public static bool SlotAcceptsValue(Slot slot, DraggableModel model)
    {
        if (slot == null || model == null)
        {
            return false;
        }
        return PrefabAcceptsClasses(slot.gameObject, model.classes) && slot.CanAcceptValue(model.GetType());
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


    private T GetFirstHit<T>(PointerEventData eventData, Vector3? localOffset = null) where T : MonoBehaviour
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
        return transform.InverseTransformPoint(canvas.transform.TransformPoint(eventData.position));
    }
    private Vector2 GetEventPointDelta(Vector3 localOffset)
    {
        return canvas.transform.InverseTransformVector(transform.TransformVector(localOffset));
    }
    #endregion
}
