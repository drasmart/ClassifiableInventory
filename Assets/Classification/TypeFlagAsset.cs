using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
