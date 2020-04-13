using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Slot : BaseSlot
{
    public int index;

    public DraggableUI draggableUI;

    public DraggableModel draggableModel {
        get {
            object model = null;
            GetAccess(targetScript, property, propertyType,
                (field, dataType) => model = field.GetValue(targetScript),
                (list, dataType) => model = (index < list.Count) ? list[index] : null,
                (array, dataType) => model = (index < array.Length) ? array.GetValue(index) : null,
                null);
            return model as DraggableModel;
        }
        set {
            GetAccess(targetScript, property, propertyType,
                (field, dataType) => field.SetValue(targetScript, value),
                (list, dataType) => { if (index < list.Count) { list[index] = value; }; },
                (array, dataType) => { if (index < array.Length) { array.SetValue(value, index); }; },
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
    public override Slot FindFreeSlotFor(DraggableModel model)
    {
        Type storageType = null;
        object storedValue = null;
        GetAccess(targetScript, property, propertyType,
            (field, dataType) => { storageType = dataType; storedValue = field.GetValue(targetScript); },
            (list, dataType) => { storageType = dataType; storedValue = (index < list.Count) ? list[index] : null; },
            (array, dataType) => { storageType = dataType; storedValue = (index < array.Length) ? array.GetValue(index) : null; },
            null);
        bool hasSpace = (storedValue == null) || ((storedValue as DraggableModel)?.IsNull == true);
        if (!(hasSpace && storageType != null && storageType.IsAssignableFrom(model.GetType())))
        {
            return null;
        }
        return this;
    }
}
