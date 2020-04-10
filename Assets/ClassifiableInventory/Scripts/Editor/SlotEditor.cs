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

    private LastDropClick lastDropClick;

    void OnEnable()
    {
        targetProp = serializedObject.FindProperty("targetScript");
        propertyProp = serializedObject.FindProperty("property");
        propertyTypeProp = serializedObject.FindProperty("propertyType");
        indexProp = serializedObject.FindProperty("index");
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
        EditorGUILayout.PropertyField(propertyProp);
        EditorGUILayout.PropertyField(propertyTypeProp);
        EditorGUILayout.PropertyField(indexProp);

        FindClassifiableFields();

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
        if (!EditorGUILayout.DropdownButton(new GUIContent(propertyProp.stringValue), FocusType.Keyboard))
        {
            return;
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

        string msg = t.Name + "\n";
        foreach (var nextField in t.GetFields())
        {
            var nextType = nextField.FieldType;
            var name = nextField.Name;
            bool active = name == propertyProp.stringValue;

            msg += nextField.Name + " (" + nextType.Name + "): can ";
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
}
