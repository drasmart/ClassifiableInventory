using System.Collections;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.Assertions;

#nullable enable

[CustomEditor(typeof(Slot))]
[CanEditMultipleObjects]
public class SlotEditor : BaseSlotEditor
{
    private SerializedProperty? indexProp;
    private SerializedProperty? draggableContainerProp;

    private SerializedProperty? draggableUIProp;

    private int? newPropLength;

    protected override void OnEnable()
    {
        base.OnEnable();
        draggableContainerProp = serializedObject.FindProperty("draggableContainer");
        indexProp = serializedObject.FindProperty("index");

        draggableUIProp = serializedObject.FindProperty("draggableUI");
    }

    protected override void OnSlotInspection()
    {
        Assert.IsNotNull(indexProp);
        if (newPropLength != null)
        {
            indexProp!.intValue = newPropLength.Value;
            newPropLength = null;
        }

        EditorGUILayout.PropertyField(draggableContainerProp);

        base.OnSlotInspection();

        EditorGUILayout.PropertyField(draggableUIProp);
    }

    protected override void DrawReflectedProperty()
    {
        base.DrawReflectedProperty();

        ShowIndexPicker();
    }

    protected override PropertyPickHandler PickListField(string fieldName, IList list) => () =>
    {
        base.PickListField(fieldName, list);
        newPropLength = list.Count;
    };

    protected override PropertyPickHandler PickArrayField(string fieldName, Array array) => () =>
    {
        base.PickArrayField(fieldName, array);
        newPropLength = array.Length;
    };

    private void ShowIndexPicker()
    {
        Assert.IsNotNull(PropertyProp);
        Assert.IsNotNull(PropertyTypeProp);
        Assert.IsNotNull(indexProp);
        var targ = (serializedObject.targetObject as Slot)?.targetScript;
        var propName = PropertyProp!.stringValue;
        var propType = (SlotPropertyType)PropertyTypeProp!.intValue;
        int len = -1;
        BaseSlot.GetAccess(targ, propName, propType, null, (list, _) => len = list.Count, (array, _) => len = array.Length, null);
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
        var index = Mathf.Clamp(indexProp!.intValue, 0, len - 1);
        indexProp.intValue = EditorGUILayout.IntSlider($"{prefixLabel} (0-{len-1})", index, 0, len - 1);
    }
}
