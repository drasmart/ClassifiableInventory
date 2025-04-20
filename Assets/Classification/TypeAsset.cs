using System;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace drasmart.Classification
{
    [Serializable]
    public abstract class TypeAsset<TAsset> : Classifiable.TypeAsset 
        where TAsset: TypeAsset<TAsset>
    {
        [SerializeField]
        private List<TAsset> supertypes = new();

        public bool IsSubtypeOf(TypeAsset<TAsset> other)
        {
            if (!other || this == other)
            {
                return true;
            }
            var tested = new HashSet<TypeAsset<TAsset>>();
            var toTest = new LinkedList<TypeAsset<TAsset>>();
            toTest.AddFirst(this);
            tested.Add(this);
            while (toTest.Count > 0)
            {
                var next = toTest.First.Value;
                toTest.RemoveFirst();
                foreach(var supertype in next.supertypes)
                {
                    if(supertype == other)
                    {
                        return true;
                    }
                    if(tested.Contains(supertype))
                    {
                        continue;
                    }
                    toTest.AddLast(supertype);
                    tested.Add(supertype);
                }
            }
            return false;
        }

        public bool IsSupertypeOf(TypeAsset<TAsset> other)
        {
            return other && other.IsSubtypeOf(this);
        }

        public sealed override bool Filter(Classifiable.TypeAsset typeAsset)
        {
            return typeAsset is TAsset other && IsSupertypeOf(other);
        }
    }
}
