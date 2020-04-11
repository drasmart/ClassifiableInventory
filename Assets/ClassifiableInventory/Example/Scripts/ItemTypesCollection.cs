using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItemTypesCollection", menuName = "Inventory/Item Types Collection")]
public class ItemTypesCollection : ScriptableObject
{
    public ItemType[] itemTypes;
}
