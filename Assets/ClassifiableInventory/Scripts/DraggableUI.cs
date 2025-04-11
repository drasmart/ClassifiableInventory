using UnityEngine;
using UnityEngine.Events;
using Classification;

#nullable enable

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Classifiable))]
public class DraggableUI : MonoBehaviour
{
    public Slot? slot;
    public IDraggableModel? DraggableModel;
    public bool isShadow;

    public ModelUpdatedEvent? onModelUpdate;

    [System.Serializable] public class ModelUpdatedEvent : UnityEvent<IDraggableModel, bool> { }
}
