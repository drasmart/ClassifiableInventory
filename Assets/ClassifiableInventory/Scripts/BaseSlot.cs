using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Classification;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Classifiable))]
public abstract class BaseSlot : FallbackSlotContainer
{
    public MonoBehaviour targetScript;
    public string property;
    public SlotPropertyType propertyType;

    [System.Serializable]
    public enum DragMode
    {
        DetachOnDrag = 0,
        BounceBackOnDrop,
        KeepShadowWhileDragging,
    }
    public DragMode dragMode = DragMode.BounceBackOnDrop;
    public FallbackSlotContainer fallbackSlotContainer;

    public delegate void FieldHandler(System.Reflection.FieldInfo field, Type dataType);
    public delegate void ListHandler(IList collection, Type dataType);
    public delegate void ArrayHandler(Array array, Type dataType);
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
                    fieldHandler?.Invoke(field, fieldType);
                    return;
                case SlotPropertyType.Array:
                    {
                        var elementType = fieldType.GetElementType();
                        if (fieldType.IsArray && q.IsAssignableFrom(elementType))
                        {
                            arrayHandler?.Invoke(field.GetValue(target) as Array, elementType);
                            return;
                        }
                    }
                    break;
                case SlotPropertyType.List:
                    if (fieldType.IsGenericType)
                    {
                        var genericArgs = fieldType.GenericTypeArguments;
                        if (genericArgs.Length != 1)
                        {
                            break;
                        }
                        var elementType = genericArgs[0];
                        if (q.IsAssignableFrom(elementType))
                        {
                            listHandler?.Invoke(field.GetValue(target) as IList, elementType);
                            return;
                        }
                    }
                    break;
            }
        } while (false);
        failHandler?.Invoke();
    }

    public virtual bool CanAcceptValue(Type modelType)
    {
        Type storageType = null;
        GetAccess(targetScript, property, propertyType,
            (field, dataType) => storageType = dataType,
            (list, dataType) => storageType = dataType,
            (array, dataType) => storageType = dataType,
            null);
        return storageType != null && storageType.IsAssignableFrom(modelType);
    }
}
