using UnityEngine;

#nullable enable

namespace drasmart.Classification.Colors
{
    [CreateAssetMenu(menuName = "Classification/Color Type")]
    public class ColorTypeAsset : TypeAsset<ColorTypeAsset>
    {
        public Color color = Color.black;
    }
}
