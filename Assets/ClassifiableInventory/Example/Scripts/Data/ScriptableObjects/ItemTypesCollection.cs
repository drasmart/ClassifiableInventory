using System;
using UnityEngine;

#nullable enable

[CreateAssetMenu(menuName = "Inventory/Item Types Collection")]
public class ItemTypesCollection : ScriptableObject
{
    public ItemType[] itemTypes = Array.Empty<ItemType>();
}
