using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Classification;
using System;

[CustomEditor(typeof(Slot))]
[CanEditMultipleObjects]
public class SlotEditor : BaseSlotEditor
{
    private SerializedProperty indexProp;

    private SerializedProperty draggableUIProp;

    private int? newPropLength;

    protected override void OnEnable()
    {
        base.OnEnable();
        indexProp = serializedObject.FindProperty("index");

        draggableUIProp = serializedObject.FindProperty("draggableUI");
    }

    protected override void OnSlotInspection()
    {
        if (newPropLength != null)
        {
            indexProp.intValue = newPropLength.Value;
            newPropLength = null;
        }

        base.OnSlotInspection();

        EditorGUILayout.PropertyField(draggableUIProp);
    }

    protected override void DrawReflectedProperty()
    {
        base.DrawReflectedProperty();

        ShowIndexPicker();
    }

    protected override PropertyPickHandler PickListField(string name, IList list) => () =>
    {
        base.PickListField(name, list);
        newPropLength = list.Count;
    };

    protected override PropertyPickHandler PickArrayField(string name, Array array) => () =>
    {
        base.PickArrayField(name, array);
        newPropLength = array.Length;
    };

    private void ShowIndexPicker()
    {
        var targ = (serializedObject.targetObject as Slot)?.targetScript;
        var propName = propertyProp.stringValue;
        var propType = (SlotPropertyType)propertyTypeProp.intValue;
        int len = -1;
        BaseSlot.GetAccess(targ, propName, propType, null, (list, dataType) => len = list.Count, (array, dataType) => len = array.Length, null);
        if (len == -1)
        {
            return;
        }
        const string prefixLabel = "Element Index";
        if (len == 0)
        {
            EditorGUILayout.LabelField(prefixLabel, "Collection size is 0", EditorStyles.textField);
            return;
        }
        var index = Mathf.Clamp(indexProp.intValue, 0, len - 1);
        indexProp.intValue = EditorGUILayout.IntSlider(prefixLabel + " (0-" + (len-1).ToString() + ")", index, 0, len - 1);
    }
}
