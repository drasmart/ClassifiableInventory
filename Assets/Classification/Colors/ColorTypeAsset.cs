using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Classification;

[CreateAssetMenu(fileName = "ColorType.asset", menuName = "Classification/Color Type")]
public class ColorTypeAsset : TypeAsset<ColorTypeAsset>
{
    public Color color = Color.black;
}
