using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Classification
{
    [Serializable]
    public abstract class TypeAsset<T> : Classifiable.TypeAsset where T: TypeAsset<T>
    {
        public List<T> supertypes = new List<T>();

        public bool IsSubtypeOf(TypeAsset<T> other)
        {
            if (other == null || this == other)
            {
                return true;
            }
            var tested = new HashSet<TypeAsset<T>>();
            var toTest = new LinkedList<TypeAsset<T>>();
            toTest.AddFirst(this);
            tested.Add(this);
            while (toTest.Count > 0)
            {
                var next = toTest.First.Value;
                toTest.RemoveFirst();
                if(next.supertypes == null)
                {
                    continue;
                }
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

        public bool IsSupertypeOf(TypeAsset<T> other)
        {
            return other != null && other.IsSubtypeOf(this);
        }

        public sealed override bool Filter(Classifiable.TypeAsset typeAsset)
        {
            return IsSupertypeOf(typeAsset as T);
        }
    }
}
