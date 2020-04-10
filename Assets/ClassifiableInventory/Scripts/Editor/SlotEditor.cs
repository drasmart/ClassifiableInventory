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

        EditorGUILayout.PropertyField(targetProp);
        EditorGUILayout.PropertyField(propertyProp);
        EditorGUILayout.PropertyField(propertyTypeProp);
        EditorGUILayout.PropertyField(indexProp);

        FindClassifiableFields();

        serializedObject.ApplyModifiedProperties();
    }

    private void FindClassifiableFields()
    {
        var targ = (serializedObject.targetObject as Slot)?.targetScript;
        if (targ == null)
        {
            return;
        }
        var t = targ.GetType();
        var q = typeof(DraggableModel);
        var qName = q.Name;

        string msg = t.Name + "\n";
        foreach(var nextField in t.GetFields())
        {
            var nextType = nextField.FieldType;

            msg += nextField.Name + " (" + nextType.Name + "): can ";
            if (!q.IsAssignableFrom(nextType))
            {
                msg += "not ";
            }
            msg += "assign to " + qName;
            msg += "\n";

            if (nextType.IsGenericType)
            {
                var genericArgs = nextType.GetGenericArguments();
                if (genericArgs.Length == 1 && nextType == typeof(List<>).MakeGenericType(genericArgs[0]))
                {
                    var testType = genericArgs[0];
                    msg += nextField.Name + " (List<" + testType.Name + ">): can ";
                    if (!q.IsAssignableFrom(testType))
                    {
                        msg += "not ";
                    }
                    msg += "assign elements to " + qName;
                    msg += "\n";
                }
            }

            if (nextType.IsArray)
            {
                var testType = nextType.GetElementType();
                msg += nextField.Name + " (" + testType.Name + "[]): can ";
                if (!q.IsAssignableFrom(testType))
                {
                    msg += "not ";
                }
                msg += "assign elements to " + qName;
                msg += "\n";
            }
        }
        Debug.Log(msg);
    }
}
