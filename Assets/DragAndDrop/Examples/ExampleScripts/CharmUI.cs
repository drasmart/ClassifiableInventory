using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DragAndDrop;

using System;

// specialisation of Draggable that displays charms
public class CharmUI : Draggable, IToolTip
{
    public Image image;

    public string getToolTipMessage()
    {
        Charm charm = obj as Charm;
        return (charm == null) ? "" : charm.GetDescription();
    }

    public override void UpdateObject()
    {
        Charm charm = obj as Charm;

        if (charm == null && obj != null)
        {
            Debug.LogWarning("Trying to place something that isn't a Charm into a CharmUI!");
        }

        // set the visible data
        if (charm)
        {
            image.sprite = charm.icon;
            image.color = charm.color;
        }

        // turn off if it was null
        gameObject.SetActive(charm != null);
    }
}
