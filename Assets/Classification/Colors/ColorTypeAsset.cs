using UnityEngine;
using Classification;

#nullable enable

[CreateAssetMenu(menuName = "Classification/Color Type")]
public class ColorTypeAsset : TypeAsset<ColorTypeAsset>
{
    public Color color = Color.black;
}
