using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DragAndDrop;

public class PowerUI : Draggable {

    public Image icon;
    public Text label;

    public override void UpdateObject()
    {
        Power p = obj as Power;

        // if we have a power
        if (p != null)
        {
            // set the icon
            if (icon)
            {
                icon.sprite = p.icon;
                icon.color = p.color;
            }
            // set the label
            if (label)
                label.text = p.name;
        }
        // turn off if there is no Power
        gameObject.SetActive(p != null);
    }
}
