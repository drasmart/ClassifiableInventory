using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragAndDrop;

public class PowersUI : ObjectContainerList<Power> {

    public Player player;

	// Use this for initialization
	void Start () {
        CreateSlots(player.powers);
	}
}
