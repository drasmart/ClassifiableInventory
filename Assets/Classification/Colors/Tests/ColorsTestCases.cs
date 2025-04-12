using System.Collections.Generic;
using System.Linq;
using Classification;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Serialization;

#nullable enable

[CreateAssetMenu(menuName = "Classification/Tests/Colors Test Cases")]
public class ColorsTestCases : ScriptableObject
{
    public List<TestCaseData> testCases = new();

    [System.Serializable]
    public class TestCaseData
    {
        public ColorTypeAsset? color;
        public List<ColorExpectation> colors = new();
        public List<FilterExpectation> filters = new();

        [System.Serializable]
        public class ColorExpectation
        {
            public ColorTypeAsset? color;
            public bool expected;
        }

        [System.Serializable]
        public class FilterExpectation
        {
            public ColorTypeFilterAsset? filter;
            public bool expected;
        }
    }

    public IEnumerable<ColorsTestCase> AllCases => testCases.SelectMany(
        x =>
        {
            Assert.IsNotNull(x.color);
            return x.colors.Select(c =>
            {
                Assert.IsNotNull(c.color);
                return new ColorsTestCase(x.color!, c.color!, c.expected);
            }).Concat(x.filters.Select(f =>
            {
                Assert.IsNotNull(f.filter);
                return new ColorsTestCase(x.color!, f.filter!, f.expected);
            }));
        });
}
