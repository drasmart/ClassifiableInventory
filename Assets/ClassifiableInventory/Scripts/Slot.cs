using System;
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

    public DraggableModel draggableModel {
        get {
            object model = null;
            GetAccess(targetScript, property, propertyType,
                (field) => model = field.GetValue(targetScript),
                (list) => model = (index < list.Count) ? list[index] : null,
                (array) => model = (index < array.Length) ? array.GetValue(index) : null,
                null);
            return model as DraggableModel;
        }
        set {
            GetAccess(targetScript, property, propertyType,
                (field) => field.SetValue(targetScript, value),
                (list) => { if (index < list.Count) { list[index] = value; }; },
                (array) => { if (index < array.Length) { array.SetValue(value, index); }; },
                null);
        }
    }

    private void OnEnable()
    {
        DragManager.Instance.UpdateSlot(this, true);
    }
    private void OnDisable()
    {
        DragManager.Instance.UpdateSlot(this, false);
    }

    public delegate void FieldHandler(System.Reflection.FieldInfo field);
    public delegate void ListHandler(IList collection);
    public delegate void ArrayHandler(Array array);
    public delegate void FailHandler();

    public static void GetAccess(MonoBehaviour target, string property, PropertyType propertyType, FieldHandler fieldHandler, ListHandler listHandler, ArrayHandler arrayHandler, FailHandler failHandler)
    {
        do
        {
            if (target == null || string.IsNullOrEmpty(property))
            {
                break;
            }
            var t = target.GetType();
            var field = t.GetField(property);
            if (field == null)
            {
                break;
            }
            var fieldType = field.FieldType;
            var q = typeof(DraggableModel);
            switch (propertyType)
            {
                case Slot.PropertyType.Plain:
                    fieldHandler?.Invoke(field);
                    return;
                case Slot.PropertyType.Array:
                    if (fieldType.IsArray && q.IsAssignableFrom(fieldType.GetElementType()))
                    {
                        arrayHandler?.Invoke(field.GetValue(target) as Array);
                        return;
                    }
                    break;
                case Slot.PropertyType.List:
                    if (fieldType.IsGenericType)
                    {
                        var genericArgs = fieldType.GenericTypeArguments;
                        if (genericArgs.Length == 1 && q.IsAssignableFrom(genericArgs[0]))
                        {
                            listHandler?.Invoke(field.GetValue(target) as IList);
                            return;
                        }
                    }
                    break;
            }
        } while (false);
        failHandler?.Invoke();
    }
}
