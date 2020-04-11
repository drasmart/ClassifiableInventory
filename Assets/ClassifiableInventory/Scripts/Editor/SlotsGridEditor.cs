using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Classification;
using System;

[CustomEditor(typeof(SlotsGrid))]
[CanEditMultipleObjects]
public class SlotsGridEditor : BaseSlotEditor
{
    private SerializedProperty slotPrefabProp;

    protected override void OnEnable()
    {
        base.OnEnable();
        slotPrefabProp = serializedObject.FindProperty("slotPrefab");
    }

    protected override void OnSlotInspection()
    {
        EditorGUILayout.PropertyField(slotPrefabProp);
        base.OnSlotInspection();
    }

    protected override PropertyPickHandler PickPlainField(string name, System.Reflection.FieldInfo field) => null;
}
