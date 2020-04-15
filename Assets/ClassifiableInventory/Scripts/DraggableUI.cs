using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Classification;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Classifiable))]
public class DraggableUI : MonoBehaviour
{
    public Slot slot;
    public DraggableModel draggableModel;
    public bool isShadow;

    public ModelUpdatedEvent onModelUpdate;

    [System.Serializable] public class ModelUpdatedEvent : UnityEvent<DraggableModel, bool> { }
}
