using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Classification;

[CreateAssetMenu(menuName = "Classification/Color Type")]
public class ColorTypeAsset : TypeAsset<ColorTypeAsset>
{
    public Color color = Color.black;
}
