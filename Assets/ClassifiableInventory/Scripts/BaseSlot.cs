using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Classification;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Classifiable))]
public class BaseSlot : MonoBehaviour
{
    public MonoBehaviour targetScript;
    public string property;
    public SlotPropertyType propertyType;

    public delegate void FieldHandler(System.Reflection.FieldInfo field);
    public delegate void ListHandler(IList collection);
    public delegate void ArrayHandler(Array array);
    public delegate void FailHandler();

    public static void GetAccess(MonoBehaviour target, string property, SlotPropertyType propertyType, FieldHandler fieldHandler, ListHandler listHandler, ArrayHandler arrayHandler, FailHandler failHandler)
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
                case SlotPropertyType.Plain:
                    fieldHandler?.Invoke(field);
                    return;
                case SlotPropertyType.Array:
                    if (fieldType.IsArray && q.IsAssignableFrom(fieldType.GetElementType()))
                    {
                        arrayHandler?.Invoke(field.GetValue(target) as Array);
                        return;
                    }
                    break;
                case SlotPropertyType.List:
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
