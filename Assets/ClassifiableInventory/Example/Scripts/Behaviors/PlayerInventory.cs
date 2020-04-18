using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInventory : MonoBehaviour
{
    [Header("Items")]
    public List<Item> backpack;

    public Item weapon;

    public Item body;
    public Item helmet;

    public Item[] potions;

    [Header("Skills")]
    public List<Skill> knownSkills;

    public Skill activeSkill;
    public Skill passiveSkill;
    public Skill aura;

    [Header("Events")]
    public UnityEvent onDataUpdate;
}
