using System;
using System.Collections.Generic;

#nullable enable

namespace drasmart.Classification
{
    [Serializable]
    public class TypeFilterAsset<TAsset, TFilterAsset> : Classifiable.TypeFilter
        where TAsset : TypeAsset<TAsset>
        where TFilterAsset : TypeFilterAsset<TAsset, TFilterAsset>
    {
        public bool isBlacklist;
        public bool requireAll;

        public List<TAsset> requiredTypes = new();
        public List<TFilterAsset> requiredFilters = new();

        public sealed override bool Filter(Classifiable.TypeAsset typeAsset)
        {
            if (typeAsset is TAsset other && other)
            {
                bool? result = null;
                if ((result = CheckFilters(other, requiredTypes)) is not null) { return result.Value; }
                if ((result = CheckFilters(other, requiredFilters)) is not null) { return result.Value; }
                return isBlacklist ^ requireAll;
            }
            return false;
        }

        private bool? CheckFilters<TC>(TypeAsset<TAsset> typeAsset, List<TC> requirements) where TC: Classifiable.TypeFilter
        {
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
