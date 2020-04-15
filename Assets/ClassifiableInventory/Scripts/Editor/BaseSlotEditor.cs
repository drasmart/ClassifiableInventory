using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Classification;
using System;

public class BaseSlotEditor : Editor
{
    protected SerializedProperty targetProp;
    protected SerializedProperty propertyProp;
    protected SerializedProperty propertyTypeProp;
    protected SerializedProperty fallbackSlotProp;
    protected SerializedProperty dragModeProp;

    private bool unfoldReflectedProperty = true;
    private LastDropClick lastDropClick;

    protected virtual void OnEnable()
    {
        targetProp = serializedObject.FindProperty("targetScript");
        propertyProp = serializedObject.FindProperty("property");
        propertyTypeProp = serializedObject.FindProperty("propertyType");
        fallbackSlotProp = serializedObject.FindProperty("fallbackSlotContainer");
        dragModeProp = serializedObject.FindProperty("dragMode");
    }

    protected delegate void PropertyPickHandler();

    protected virtual PropertyPickHandler PickPlainField(string name, System.Reflection.FieldInfo field) => () => lastDropClick = new LastDropClick(name, SlotPropertyType.Plain);
    protected virtual PropertyPickHandler PickListField (string name, IList list) => () => lastDropClick = new LastDropClick(name, SlotPropertyType.List);
    protected virtual PropertyPickHandler PickArrayField(string name, Array array) => () => lastDropClick = new LastDropClick(name, SlotPropertyType.Array);

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        OnSlotInspection();

        serializedObject.ApplyModifiedProperties();
    }

    protected virtual void OnSlotInspection()
    {
        if (lastDropClick != null)
        {
            propertyProp.stringValue = lastDropClick.property;
            propertyTypeProp.enumValueIndex = (int)lastDropClick.propertyType;
            lastDropClick = null;
        }

        if (unfoldReflectedProperty = EditorGUILayout.Foldout(unfoldReflectedProperty, "Reflected Data"))
        {
            using (var h = new EditorGUI.IndentLevelScope(1))
            {
                EditorGUILayout.PropertyField(targetProp);

                DrawReflectedProperty();
            }
        }

        EditorGUILayout.PropertyField(dragModeProp);
        EditorGUILayout.PropertyField(fallbackSlotProp);
    }

    protected virtual void DrawReflectedProperty()
    {
        using (var h = new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.PrefixLabel(new GUIContent("Reflected Field"));
            if (!EditorGUILayout.DropdownButton(new GUIContent(propertyProp.stringValue), FocusType.Keyboard))
            {
                return;
            }
        }

        var targ = (serializedObject.targetObject as BaseSlot)?.targetScript;
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
                AddDropdownOption(menu, name, active, PickPlainField(name, nextField));
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
                        AddDropdownOption(menu, name, active, PickListField(name, nextField.GetValue(targ) as IList));
                        continue;
                    }
                }
            }

            if (nextType.IsArray)
            {
                var testType = nextType.GetElementType();
                if (q.IsAssignableFrom(testType))
                {
                    AddDropdownOption(menu, name, active, PickArrayField(name, nextField.GetValue(targ) as Array));
                    continue;
                }
            }
        }

        menu.ShowAsContext();
    }

    private void AddDropdownOption(GenericMenu menu, string name, bool active, PropertyPickHandler pickHandler)
    {
        if (pickHandler != null)
        {
            menu.AddItem(new GUIContent(name), active, () => pickHandler());
        }
    }

    private class LastDropClick
    {
        public readonly string property;
        public readonly SlotPropertyType propertyType;

        public LastDropClick(string property, SlotPropertyType propertyType)
        {
            this.property = property;
            this.propertyType = propertyType;
        }
    }
}
