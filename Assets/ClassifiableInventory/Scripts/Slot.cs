using System;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

public class Slot : BaseSlot
{
    public RectTransform? draggableContainer;
    public int index;

    public DraggableUI? draggableUI;

    public IDraggableModel? DraggableModel {
        get {
            object? model = null;
            GetAccess(targetScript, property, propertyType,
                (field, _) => model = field.GetValue(targetScript),
                (list, _) => model = (index < list.Count) ? list[index] : null,
                (array, _) => model = (index < array.Length) ? array.GetValue(index) : null,
                null);
            return model as IDraggableModel;
        }
        set {
            if (isReadOnly)
            {
                return;
            }
            GetAccess(targetScript, property, propertyType,
                (field, _) => field.SetValue(targetScript, value),
                (list, _) => { if (index < list.Count) { list[index] = value; } },
                (array, _) => { if (index < array.Length) { array.SetValue(value, index); } },
                null);
        }
    }

    private void OnEnable()
    {
        DragManager.Instance.UpdateSlot(this, true);
    }
    private void OnDisable()
    {
        DragManager.Instance.UpdateSlot(this, false);
    }
    public override void UpdateAllSlots()
    {
        DragManager.Instance.UpdateSlot(this, enabled);
    }
    public override Slot? FindFreeSlotFor(IDraggableModel model)
    {
        Type? storageType = null;
        object? storedValue = null;
        GetAccess(targetScript, property, propertyType,
            (field, dataType) => { storageType = dataType; storedValue = field.GetValue(targetScript); },
            (list, dataType) => { storageType = dataType; storedValue = (index < list.Count) ? list[index] : null; },
            (array, dataType) => { storageType = dataType; storedValue = (index < array.Length) ? array.GetValue(index) : null; },
            null);
        bool hasSpace = (storedValue == null) || ((storedValue as IDraggableModel)?.IsNull == true);
        if (!(hasSpace && storageType is not null && storageType.IsAssignableFrom(model.GetType())))
        {
            return null;
        }
        return this;
    }
    public override IEnumerable<Slot> GetAllSlots()
    {
        yield return this;
    }
}
