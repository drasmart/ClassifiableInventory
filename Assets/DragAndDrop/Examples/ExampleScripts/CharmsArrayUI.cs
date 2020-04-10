using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DragAndDrop;

public class CharmsArrayUI : ObjectContainerArray {

    // where the data comes from - this represents the player, and either their backpack or belt
    public Player player;
    public Player.CharmList charmList;

    public Charm.CharmType charmType = Charm.CharmType.All;
    public Text description;

    void Start()
    {
        CreateSlots(player.GetCharms(charmList));
    }

    // to be able to drop, it must be a charm of the appropriate type
    public override bool CanDrop(Draggable dragged, Slot slot)
    {
        CharmUI charm = dragged as CharmUI;

        // must be a charm
        if (charm == null)
            return false;

        Charm ch = charm.obj as Charm;

        // replacing empty slots in here is OK
        if (ch == null)
            return true;

        // check the type of items in the belt vs what we can hold
        int mask = (int)ch.charmType & (int)charmType;
        return mask != 0;
    }

    public override void OnDragBegin()
    {
        // when we start to drag an object, show its name and requirements in the text field provided
        base.OnDragBegin();
        if (description)
        {
            CharmUI charmUi = Draggable.current as CharmUI;
            if (charmUi)
            {
                Charm charm = charmUi.obj as Charm;
                if (charm)
                    description.text = charm.GetDescription();
            }
        }
    }
}
