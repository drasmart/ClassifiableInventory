using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInventory : MonoBehaviour
{
    public List<Item> backpack;

    public Item weapon;

    public Item body;
    public Item helmet;

    public Item[] potions;

    public UnityEvent onDataUpdate;
}
