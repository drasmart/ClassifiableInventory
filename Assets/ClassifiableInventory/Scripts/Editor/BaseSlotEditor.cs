using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.Assertions;

#nullable enable

public class BaseSlotEditor : Editor
{
    protected SerializedProperty? TargetProp;
    protected SerializedProperty? PropertyProp;
    protected SerializedProperty? PropertyTypeProp;
    protected SerializedProperty? FallbackSlotProp;
    protected SerializedProperty? KeepShadowWhileDraggingProp;
    protected SerializedProperty? IsReadOnlyProp;

    private bool unfoldReflectedProperty = true;
    private LastDropClick? lastDropClick;

    protected virtual void OnEnable()
    {
        TargetProp = serializedObject.FindProperty("targetScript");
        PropertyProp = serializedObject.FindProperty("property");
        PropertyTypeProp = serializedObject.FindProperty("propertyType");
        FallbackSlotProp = serializedObject.FindProperty("fallbackSlotContainer");
        KeepShadowWhileDraggingProp = serializedObject.FindProperty("keepShadowWhileDragging");
        IsReadOnlyProp = serializedObject.FindProperty("isReadOnly");
    }

    protected delegate void PropertyPickHandler();

    protected virtual PropertyPickHandler? PickPlainField(string fieldName, System.Reflection.FieldInfo _) => () => lastDropClick = new LastDropClick(fieldName, SlotPropertyType.Plain);
    protected virtual PropertyPickHandler? PickListField (string fieldName, IList list) => () => lastDropClick = new LastDropClick(fieldName, SlotPropertyType.List);
    protected virtual PropertyPickHandler? PickArrayField(string fieldName, Array array) => () => lastDropClick = new LastDropClick(fieldName, SlotPropertyType.Array);

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        OnSlotInspection();

        serializedObject.ApplyModifiedProperties();
    }

    protected virtual void OnSlotInspection()
    {
        Assert.IsNotNull(PropertyProp);
        Assert.IsNotNull(PropertyTypeProp);
        
        if (lastDropClick != null)
        {
            PropertyProp!.stringValue = lastDropClick.Property;
            PropertyTypeProp!.enumValueIndex = (int)lastDropClick.PropertyType;
            lastDropClick = null;
        }

        unfoldReflectedProperty = EditorGUILayout.Foldout(unfoldReflectedProperty, "Reflected Data");
        if (unfoldReflectedProperty)
        {
            using var h = new EditorGUI.IndentLevelScope(1);
            EditorGUILayout.PropertyField(TargetProp);

            DrawReflectedProperty();
        }

        EditorGUILayout.PropertyField(FallbackSlotProp);
        {
            using var _ = new EditorGUILayout.HorizontalScope();
            EditorGUILayout.PropertyField(KeepShadowWhileDraggingProp);
            EditorGUILayout.PropertyField(IsReadOnlyProp);
        }
    }

    protected virtual void DrawReflectedProperty()
    {
        Assert.IsNotNull(PropertyProp);
        using (var _ = new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.PrefixLabel(new GUIContent("Reflected Field"));
            if (!EditorGUILayout.DropdownButton(new GUIContent(PropertyProp!.stringValue), FocusType.Keyboard))
            {
                return;
            }
        }

        var targ = (serializedObject.targetObject as BaseSlot)?.targetScript;
        if (!targ)
        {
            return;
        }

        GenericMenu menu = new GenericMenu();

        var t = targ.GetType();
        var q = typeof(IDraggableModel);

        foreach (var nextField in t.GetFields())
        {
            var nextType = nextField.FieldType;
            var fieldName = nextField.Name;
            bool active = fieldName == PropertyProp.stringValue;

            if (q.IsAssignableFrom(nextType))
            {
                AddDropdownOption(menu, fieldName, active, PickPlainField(fieldName, nextField));
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
                        if (nextField.GetValue(targ) is IList list)
                        {
                            AddDropdownOption(
                                menu,
                                fieldName,
                                active,
                                PickListField(fieldName, list));
                        }
                        continue;
                    }
                }
            }

            if (nextType.IsArray && nextField.GetValue(targ) is Array array)
            {
                var testType = nextType.GetElementType();
                if (q.IsAssignableFrom(testType))
                {
                    AddDropdownOption(
                        menu,
                        fieldName,
                        active,
                        PickArrayField(fieldName, array));
                }
            }
        }

        menu.ShowAsContext();
    }

    private void AddDropdownOption(GenericMenu menu, string fieldName, bool active, PropertyPickHandler? pickHandler)
    {
        if (pickHandler is not null)
        {
            menu.AddItem(new GUIContent(fieldName), active, () => pickHandler());
        }
    }

    private class LastDropClick
    {
        public readonly string Property;
        public readonly SlotPropertyType PropertyType;

        public LastDropClick(string property, SlotPropertyType propertyType)
        {
            Property = property;
            PropertyType = propertyType;
        }
    }
}
