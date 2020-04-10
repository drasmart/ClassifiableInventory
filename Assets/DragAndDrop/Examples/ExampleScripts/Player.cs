using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int level;

    public Charm[] belt = new Charm[4];
    public Charm[] backpack = new Charm[20];

    public List<Power> powers = new List<Power>(4);

    [Header("Equipped Items")]
    public Charm helmet;
    public Charm amulet;
    public Charm ring1;
    public Charm ring2;
    public Charm gloves;
    public Charm boots;
    public Charm armour;

    // accessor stuff for UI to use
    public enum CharmList
    {
        Belt,
        BackPack
    };

    public Charm[] GetCharms(CharmList list)
    {
        switch (list)
        {
            case CharmList.Belt: return belt;
            case CharmList.BackPack: return backpack;
            default: return null;
        }
    }
}
