using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Classification;
using System;

[CustomEditor(typeof(Slot))]
public class SlotEditor : Editor
{
    private SerializedProperty targetProp;
    private SerializedProperty propertyProp;
    private SerializedProperty propertyTypeProp;
    private SerializedProperty indexProp;

    private SerializedProperty draggableUIProp;

    private LastDropClick lastDropClick;

    void OnEnable()
    {
        targetProp = serializedObject.FindProperty("targetScript");
        propertyProp = serializedObject.FindProperty("property");
        propertyTypeProp = serializedObject.FindProperty("propertyType");
        indexProp = serializedObject.FindProperty("index");

        draggableUIProp = serializedObject.FindProperty("draggableUI");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (lastDropClick != null)
        {
            propertyProp.stringValue = lastDropClick.property;
            propertyTypeProp.enumValueIndex = (int)lastDropClick.propertyType;
            indexProp.intValue = lastDropClick.index;
            lastDropClick = null;
        }

        EditorGUILayout.PropertyField(targetProp);

        FindClassifiableFields();
        ShowIndexPicker();

        EditorGUILayout.PropertyField(draggableUIProp);

        serializedObject.ApplyModifiedProperties();
    }

    private class LastDropClick
    {
        public readonly string property;
        public readonly Slot.PropertyType propertyType;
        public readonly int index;

        public LastDropClick(string property, Slot.PropertyType propertyType, int index)
        {
            this.property = property;
            this.propertyType = propertyType;
            this.index = index;
        }
    }

    private void FindClassifiableFields()
    {
        using (var h = new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.PrefixLabel(new GUIContent("Reflected Field"));
            if (!EditorGUILayout.DropdownButton(new GUIContent(propertyProp.stringValue), FocusType.Keyboard))
            {
                return;
            }
        }

        var targ = (serializedObject.targetObject as Slot)?.targetScript;
        if (targ == null)
        {
            return;
        }

        GenericMenu menu = new GenericMenu();

        var t = targ.GetType();
        var q = typeof(DraggableModel);
        var qName = q.Name;

        foreach (var nextField in t.GetFields())
        {
            var nextType = nextField.FieldType;
            var name = nextField.Name;
            bool active = name == propertyProp.stringValue;

            if (q.IsAssignableFrom(nextType))
            {
                menu.AddItem(new GUIContent(name), active, () =>
                {
                    lastDropClick = new LastDropClick(name, Slot.PropertyType.Plain, 0);
                });
                continue;
            }

            if (nextType.IsGenericType)
            {
                var genericArgs = nextType.GetGenericArguments();
                if (genericArgs.Length == 1 && nextType == typeof(List<>).MakeGenericType(genericArgs[0]))
                {
                    var testType = genericArgs[0];
                    if (q.IsAssignableFrom(testType))
                    {
                        menu.AddItem(new GUIContent(name), active, () =>
                        {
                            lastDropClick = new LastDropClick(name, Slot.PropertyType.List, 0);
                        });
                        continue;
                    }
                }
            }

            if (nextType.IsArray)
            {
                var testType = nextType.GetElementType();
                if (q.IsAssignableFrom(testType))
                {
                    menu.AddItem(new GUIContent(name), active, () =>
                    {
                        lastDropClick = new LastDropClick(name, Slot.PropertyType.Array, 0);
                    });
                    continue;
                }
            }
        }

        menu.ShowAsContext();
    }

    private void ShowIndexPicker()
    {
        var targ = (serializedObject.targetObject as Slot)?.targetScript;
        var propName = propertyProp.stringValue;
        var propType = (Slot.PropertyType)propertyTypeProp.intValue;
        int len = -1;
        Slot.GetAccess(targ, propName, propType, null, (list) => len = list.Count, (array) => len = array.Length, null);
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
