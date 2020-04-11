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
                (field) => model = field.GetValue(targetScript),
                (list) => model = (index < list.Count) ? list[index] : null,
                (array) => model = (index < array.Length) ? array.GetValue(index) : null,
                null);
            return model as DraggableModel;
        }
        set {
            GetAccess(targetScript, property, propertyType,
                (field) => field.SetValue(targetScript, value),
                (list) => { if (index < list.Count) { list[index] = value; }; },
                (array) => { if (index < array.Length) { array.SetValue(value, index); }; },
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
}
