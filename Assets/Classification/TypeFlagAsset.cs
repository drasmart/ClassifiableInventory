using UnityEngine;

#nullable enable

namespace Classification
{
    [CreateAssetMenu(menuName = "Classification/Type Flag")]
    public class TypeFlagAsset : Classifiable.TypeAsset
    {
        public override bool Filter(Classifiable.TypeAsset typeAsset)
        {
            return ReferenceEquals(this, typeAsset);
        }
    }
}
