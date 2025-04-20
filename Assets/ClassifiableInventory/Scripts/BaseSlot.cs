using System;
using System.Collections;
using UnityEngine;
using drasmart.Classification;

#nullable enable

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Classifiable))]
public abstract class BaseSlot : FallbackSlotContainer
{
    public MonoBehaviour? targetScript;
    public string? property;
    public SlotPropertyType propertyType;

    public FallbackSlotContainer? fallbackSlotContainer;
    public bool keepShadowWhileDragging;
    public bool isReadOnly;

    public delegate void FieldHandler(System.Reflection.FieldInfo field, Type dataType);
    public delegate void ListHandler(IList collection, Type dataType);
    public delegate void ArrayHandler(Array array, Type dataType);
    public delegate void FailHandler();

    public static void GetAccess(
        MonoBehaviour? target, 
        string? property, 
        SlotPropertyType propertyType,
        FieldHandler? fieldHandler, 
        ListHandler? listHandler,
        ArrayHandler? arrayHandler, 
        FailHandler? failHandler)
    {
        do
        {
            if (!target || string.IsNullOrEmpty(property))
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
            var q = typeof(IDraggableModel);
            switch (propertyType)
            {
                case SlotPropertyType.Plain:
                    fieldHandler?.Invoke(field, fieldType);
                    return;
                case SlotPropertyType.Array:
                    {
                        if (fieldType.GetElementType() is { } elementType 
                            && fieldType.IsArray
                            && q.IsAssignableFrom(elementType))
                        {
                            if (field.GetValue(target) is Array arr)
                            {
                                arrayHandler?.Invoke(arr, elementType);
                            }

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
                        if (field.GetValue(target) is IList list && q.IsAssignableFrom(elementType))
                        {
                            listHandler?.Invoke(list, elementType);
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
        Type? storageType = null;
        GetAccess(targetScript, property, propertyType,
            (_, dataType) => storageType = dataType,
            (_, dataType) => storageType = dataType,
            (_, dataType) => storageType = dataType,
            null);
        return storageType?.IsAssignableFrom(modelType) ?? false;
    }
}
