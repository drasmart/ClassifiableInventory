using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class Slot : MonoBehaviour
{
    public MonoBehaviour targetScript;
    public string property;
    public PropertyType propertyType;
    public int index;

    public DraggableUI draggableUI;

    [System.Serializable]
    public enum PropertyType
    {
        Plain,
        Array,
        List,
    }
}
