using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#nullable enable

public class PlayerInventory : MonoBehaviour
{
    [Header("Items")]
    public List<Item?> backpack = new();

    public Item? weapon;

    public Item? body;
    public Item? helmet;

    public Item[] potions = Array.Empty<Item>();

    [Header("Skills")]
    public List<Skill> knownSkills = new();

    public Skill? activeSkill;
    public Skill? passiveSkill;
    public Skill? aura;

    [Header("Events")]
    public UnityEvent? onDataUpdate;
}
