using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item Types Collection")]
public class ItemTypesCollection : ScriptableObject
{
    public ItemType[] itemTypes;
}
