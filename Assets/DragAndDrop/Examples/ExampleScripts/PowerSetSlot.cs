using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DragAndDrop;

public class PowerSetSlot : Slot
{
    public Text powerName;
    public Text powerDesc;

    // Update is called once per frame
    public override void UpdateSlot()
    {
        PowerUI powerUI = item as PowerUI;
        if (powerUI)
        {
            Power power = powerUI.obj as Power;
            if (powerName)
                powerName.text = power ? power.name : "";
            if (powerDesc)
                powerDesc.text = power ? power.GetDescription() : "";
        }
    }
}
