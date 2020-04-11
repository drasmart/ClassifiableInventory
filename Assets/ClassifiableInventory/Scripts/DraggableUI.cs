using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Classification;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Classifiable))]
public class DraggableUI : MonoBehaviour
{
    public Slot slot;
    public DraggableModel draggableModel;
}
