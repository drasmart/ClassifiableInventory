using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;

#nullable enable

public class ColorsTests
{
    // A Test behaves as an ordinary method
    [Test]
    public void ColorsTestsSimplePasses()
    {
        Assert.AreEqual(1, LoadScriptableObject<ColorsTestCases>().Count());
    }

    [Test]
    public void ColorsTestsFromAssets([ValueSource(nameof(AllTestCases))] ColorsTestCase testCase)
    {
        Assert.AreEqual(testCase.Expected, testCase.Filter.Filter(testCase.Color));
    }

    public static IEnumerable AllTestCases => 
        LoadScriptableObject<ColorsTestCases>().SelectMany(x => x.AllCases);

    // // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // // `yield return null;` to skip a frame.
    // [UnityTest]
    // public IEnumerator ColorsTestsWithEnumeratorPasses()
    // {
    //     // Use the Assert class to test conditions.
    //     // Use yield to skip a frame.
    //     yield return null;
    // }
    
    private static IEnumerable<T> LoadScriptableObject<T>() where T : ScriptableObject
    {
        string scriptableObjectName = typeof(T).Name;
        string[] guids = AssetDatabase.FindAssets($"t:{scriptableObjectName}");
        if (guids.Length == 0)
        {
            Assert.Fail($"No {scriptableObjectName} found named {scriptableObjectName}");
        }
        foreach (var guid in guids)
        {
            if (AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(T)) is T nextAsset)
            {
                yield return nextAsset;
            }
        }
    }
}
