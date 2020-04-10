using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Power", menuName = "Power")]
public class Power : ScriptableObject {

    public Sprite icon;
    public Color color = Color.white;

    // in a properly fleshed out class, you could use the powers stats to produce something meaningful here
    public string GetDescription()
    {
        return "A power called " + name;
    }
}
