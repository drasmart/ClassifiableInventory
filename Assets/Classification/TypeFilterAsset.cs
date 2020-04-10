using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Classification
{
    [Serializable]
    public class TypeFilterAsset<T, F> : Classifiable.TypeFilter where F : TypeFilterAsset<T, F> where T : TypeAsset<T>
    {
        public bool isBlacklist = false;
        public bool requireAll = false;

        public List<T> requiredTypes = new List<T>();
        public List<F> requiredFilters = new List<F>();

        public sealed override bool Filter(Classifiable.TypeAsset typeAsset)
        {
            T other = typeAsset as T;
            if (other == null)
            {
                return false;
            }
            bool? result = null;
            result = CheckFilters(other, requiredTypes);
            if (result != null) { return result.Value; }
            result = CheckFilters(other, requiredFilters);
            if (result != null) { return result.Value; }
            return isBlacklist ^ requireAll;
        }

        private bool? CheckFilters<C>(TypeAsset<T> typeAsset, List<C> requirements) where C: Classifiable.TypeFilter
        {
            if (requirements == null)
            {
                return null;
            }
            foreach (var nextFilter in requirements)
            {
                var passed = nextFilter.Filter(typeAsset);
                if (requireAll != passed)
                {
                    return isBlacklist ^ passed;
                }
            }
            return null;
        }
    }
}
